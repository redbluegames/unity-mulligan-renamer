namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Utility class that interfaces with the StyleCop .Settings XML file
    /// </summary>
    internal class XMLTool
    {
        //// Consts =============================================================================================================

        private static IgnoreListType ignoreListStorageMode = IgnoreListType.GeneratedFileFilter;

        /// <summary>
        /// RegEx to prefix ignored filenames with when using 'GeneratedFileFilter' IgnoreList mode.
        /// </summary>
        /// <remarks>
        /// This RegEx is needed because StyleCop performs a partial string match for each entry in the GeneratedFileFilter.  For
        /// example, an entry of "Window.cs" would also ignore "AttackWindow.cs".  Since this is not intended, we're taking
        /// advantage of the fact that StyleCop (thankfully) uses RegEx to perform its filtering.  By prefixing all filename
        /// entries with this RegEx, StyleCop's filtering won't include any other matches that have text/space preceeding the
        /// filename - so, "[^a-z ]Window.cs" will only find the filename of "Window.cs", and not "BlahBlahWindow.cs".
        /// </remarks>
        private static string regexPrefixForGeneratedFileFilterNames = @"[^a-z ]";

        //// Enums & Structs ====================================================================================================

        /// <summary>
        /// Mode for storing the list of ignored filenames in the StyleCop .Settings file
        /// </summary>
        internal enum IgnoreListType
        {
            /// <summary>
            /// The list of ignored filenames will be stored as a collection of GeneratedFileFilters
            /// </summary>
            GeneratedFileFilter,

            /// <summary>
            /// The list of ignored filenames will be stored as a SourceFileList
            /// </summary>
            SourceFileList
        }

        //// Properties =========================================================================================================

        /// <summary>
        /// Gets the storage mode for the list of ignored filenames in the StyleCop .Settings file
        /// </summary>
        internal static IgnoreListType IgnoreListStorageMode
        {
            get
            {
                return XMLTool.ignoreListStorageMode;
            }
        }

        //// Methods ============================================================================================================

        /// <summary>
        /// Gets the list of filenames of ignored source files in the supplied StyleCop .Settings XML file.
        /// </summary>
        /// <param name="xmlFilePath">Absolute path to the StyleCop .Settings XML file.</param>
        /// <returns>A string array of all the ignored filenames, with extensions (e.g. Example.txt)</returns>
        internal static string[] GetIgnoredFileNames(UniversalPath xmlFilePath)
        {
            // Load the XML file
            var xmlDocument = new XmlDocument();
            xmlDocument.Load((string)xmlFilePath);

            // Check its contents for a StyleCop Settings outer element
            var rootElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings");
            if (rootElement == null)
            {
                return new string[0];
            }

            // Get the list of ignored source filenames, depending on the storage method
            switch (ignoreListStorageMode)
            {
                case IgnoreListType.GeneratedFileFilter:
                    return GetIgnoredFileNamesFromGeneratedFileFilter(xmlDocument);

                case IgnoreListType.SourceFileList:
                    return GetIgnoredFileNamesFromSourceFileList(xmlDocument);

                default:
                    throw new System.InvalidOperationException("Invalid storage mode for the StyleCop ignore list!");
            }
        }

        /// <summary>
        /// Replaces the list of ignored filenames in the StyleCop .Settings file with a new list.
        /// </summary>
        /// <param name="xmlFilePath">Absolute path to the StyleCop .Settings XML file.</param>
        /// <param name="newFileNames">A string array containing the new list of filenames to ignore.</param>
        /// <returns>Returns true if the operation succeeds, false otherwise.</returns>
        internal static bool ReplaceIgnoreFileNames(UniversalPath xmlFilePath, string[] newFileNames)
        {
            // Load the XML file
            var xmlDocument = new XmlDocument();
            xmlDocument.Load((string)xmlFilePath);

            var rootElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings");
            if (rootElement == null)
            {
                return false;
            }

            // Replace the list of ignored source filenames, depending on the storage method
            switch (ignoreListStorageMode)
            {
                case IgnoreListType.GeneratedFileFilter:
                    ReplaceIgnoredFileNamesInGeneratedFileFilter(xmlDocument, newFileNames);
                    break;

                case IgnoreListType.SourceFileList:
                    ReplaceIgnoredFileNamesInSourceFileList(xmlDocument, newFileNames);
                    break;

                default:
                    throw new System.InvalidOperationException("Invalid storage mode for the StyleCop ignore list!");
            }

            xmlDocument.Save((string)xmlFilePath);

            return true;
        }

        /// <summary>
        /// Determines whether the supplied file path is a valid StyleCop .Settings XML file
        /// </summary>
        /// <param name="xmlFilePath">Absolute full path to the StyleCop .Settings XML file</param>
        /// <returns>Returns true if the supplied file is a valid StyleCop .Settings XML file, otherwise false</returns>
        internal static bool IsStyleCopFileValid(UniversalPath xmlFilePath)
        {
            string throwAwayString;
            return IsStyleCopFileValid(xmlFilePath, out throwAwayString);
        }

        /// <summary>
        /// Determines whether the supplied file path is a valid StyleCop .Settings XML file, providing an error message if not
        /// </summary>
        /// <param name="xmlFilePath">Absolute full path to the StyleCop .Settings XML file</param>
        /// <param name="errorMessage">String field to hold the error message</param>
        /// <returns>Returns true if the supplied file is a valid StyleCop .Settings XML file, otherwise false</returns>
        internal static bool IsStyleCopFileValid(UniversalPath xmlFilePath, out string errorMessage)
        {
            if (!xmlFilePath.IsValid)
            {
                errorMessage = "Selected file does not exist:" + Environment.NewLine + Environment.NewLine + xmlFilePath;
                return false;
            }

            var fileInfo = new FileInfo((string)xmlFilePath);
            if (fileInfo.Extension != ("." + StyleCopIgnoreUtility.StyleCopFileExtension))
            {
                errorMessage = "Selected file does not have the correct extension:" + Environment.NewLine + Environment.NewLine + xmlFilePath;
                return false;
            }

            // Try loading the XML file
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load((string)xmlFilePath);
            }
            catch
            {
                errorMessage = "Selected file is not a valid StyleCop Settings file:" + Environment.NewLine + Environment.NewLine + xmlFilePath;
                return false;
            }

            // Check its contents for a StyleCop Settings outer element
            var rootElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings");
            if (rootElement == null)
            {
                // XML doesn't seem to contain StyleCop stuff
                errorMessage = "Selected file does not contain valid StyleCop Settings contents:" + Environment.NewLine + Environment.NewLine + xmlFilePath;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static string[] GetIgnoredFileNamesFromSourceFileList(XmlDocument xmlDocument)
        {
            // Check its contents for any SourceFileList element(s)
            var fileListElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings" + "/SourceFileList");
            if (fileListElement == null)
            {
                // Settings file is a valid StyleCop file, but has never had ignore files (or any SourceFileList) set up
                return new string[0];
            }

            // Get the source filenames from the SourceFileList found above
            var ignoredFileNames = new List<string>();
            foreach (XmlElement child in fileListElement.ChildNodes)
            {
                if (child.Name == "SourceFile")
                {
                    ignoredFileNames.Add(child.InnerText);
                }
            }

            return ignoredFileNames.ToArray();
        }

        private static string[] GetIgnoredFileNamesFromGeneratedFileFilter(XmlDocument xmlDocument)
        {
            // Check its contents for any Parsers element(s)
            var parsersElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings" + "/Parsers");
            if (parsersElement == null)
            {
                // Settings file is a valid StyleCop file, but has never had ignore files (or any Parsers) set up
                return new string[0];
            }

            // Find the C# parser element within Parsers, and get its ParserSettings element
            XmlNode parserSettings = null;
            var parserElements = parsersElement.SelectNodes("Parser");
            for (int i = 0; i < parserElements.Count; ++i)
            {
                var parserElement = parserElements.Item(i);
                var parserIDAttribute = parserElement.Attributes.GetNamedItem("ParserId");

                if ((parserIDAttribute != null) && (parserIDAttribute.Value == "StyleCop.CSharp.CsParser"))
                {
                    parserSettings = parserElement.SelectSingleNode("ParserSettings");
                    break;
                }
            }

            if (parserSettings == null)
            {
                // No ParserSettings element found for the C# StyleCop parser
                return new string[0];
            }

            // Find the GeneratedFileFilters setting within ParserSettings
            var parserSettingCollectionElements = parserSettings.SelectNodes("CollectionProperty");
            for (int i = 0; i < parserSettingCollectionElements.Count; ++i)
            {
                var collectionAttribute = parserSettingCollectionElements.Item(i).Attributes.GetNamedItem("Name");
                if ((collectionAttribute != null) && (collectionAttribute.Value == "GeneratedFileFilters"))
                {
                    // Get the source filenames from the GeneratedFileFilter
                    var ignoredFileNames = new List<string>();
                    foreach (XmlElement child in parserSettingCollectionElements.Item(i).ChildNodes)
                    {
                        if (child.Name == "Value")
                        {
                            var fileName = StripRegexFromFileNameForGeneratedFileFilter(child.InnerText);
                            ignoredFileNames.Add(fileName);
                        }
                    }

                    return ignoredFileNames.ToArray();
                }
            }

            // GeneratedFileFilters was never found
            return new string[0];
        }

        private static bool ReplaceIgnoredFileNamesInSourceFileList(XmlDocument xmlDocument, string[] newFileNames)
        {
            var rootElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings");
            var fileListElement = (XmlElement)xmlDocument.SelectSingleNode("/StyleCopSettings" + "/SourceFileList");
            var childrenToSave = new List<XmlElement>();

            if (fileListElement == null)
            {
                fileListElement = xmlDocument.CreateElement("SourceFileList");
                rootElement.AppendChild(fileListElement);

                var newSettingsElement = BuildNewSourceFileListIgnoreSettingElement(xmlDocument);
                childrenToSave.Add(newSettingsElement);
            }
            else
            {
                foreach (XmlElement child in fileListElement.ChildNodes)
                {
                    if (child.Name != "SourceFile")
                    {
                        childrenToSave.Add(child);
                    }
                }

                var attributesToSave = new List<XmlAttribute>();
                foreach (XmlAttribute attribute in fileListElement.Attributes)
                {
                    attributesToSave.Add(attribute);
                }

                fileListElement.RemoveAll();

                foreach (var attribute in attributesToSave)
                {
                    fileListElement.Attributes.Append(attribute);
                }
            }

            foreach (var ignoredFileName in newFileNames)
            {
                var ignoredFileElement = xmlDocument.CreateElement("SourceFile");
                ignoredFileElement.InnerText = ignoredFileName;
                ignoredFileElement.IsEmpty = false;
                fileListElement.AppendChild(ignoredFileElement);
            }

            foreach (var child in childrenToSave)
            {
                fileListElement.AppendChild(child);
            }

            return true;
        }

        private static bool ReplaceIgnoredFileNamesInGeneratedFileFilter(XmlDocument xmlDocument, string[] newFileNames)
        {
            var rootElement = xmlDocument.SelectSingleNode("/StyleCopSettings");

            // Find or create the main Parsers section
            var parsersElement = rootElement.SelectSingleNode("Parsers");
            if (parsersElement == null)
            {
                parsersElement = xmlDocument.CreateElement("Parsers");
                rootElement.AppendChild(parsersElement);
            }

            // Try to find the C# parser element within Parsers
            XmlNode csParserElement = null;
            var parserElements = parsersElement.SelectNodes("Parser");
            for (int i = 0; i < parserElements.Count; ++i)
            {
                var parserElement = parserElements.Item(i);
                var parserIDAttribute = parserElement.Attributes.GetNamedItem("ParserId");

                if ((parserIDAttribute != null) && (parserIDAttribute.Value == "StyleCop.CSharp.CsParser"))
                {
                    csParserElement = parserElement;
                    break;
                }
            }

            // Build a new C# parser element if it wasn't found
            if (csParserElement == null)
            {
                csParserElement = xmlDocument.CreateElement("Parser");
                parsersElement.AppendChild(csParserElement);

                var attribute = xmlDocument.CreateAttribute("ParserId");
                attribute.Value = "StyleCop.CSharp.CsParser";
                csParserElement.Attributes.Append(attribute);
            }

            // Find or create the C# parser's ParserSettings element
            var settingsElement = csParserElement.SelectSingleNode("ParserSettings");
            if (settingsElement == null)
            {
                settingsElement = xmlDocument.CreateElement("ParserSettings");
                csParserElement.AppendChild(settingsElement);
            }

            // Try to find the GeneratedFileFilters setting element
            XmlNode generatedFileFiltersElement = null;
            var settingElements = settingsElement.SelectNodes("CollectionProperty");
            for (int i = 0; i < settingElements.Count; ++i)
            {
                var settingElement = settingElements.Item(i);
                var settingAttribute = settingElement.Attributes.GetNamedItem("Name");

                if ((settingAttribute != null) && (settingAttribute.Value == "GeneratedFileFilters"))
                {
                    generatedFileFiltersElement = settingElement;
                    break;
                }
            }

            // If it was found, remove it - we'll make a new empty one
            if (generatedFileFiltersElement != null)
            {
                settingsElement.RemoveChild(generatedFileFiltersElement);
            }

            // Make a new GeneratedFileFilters settings element and fill it with ignored filenames
            generatedFileFiltersElement = BuildNewGeneratedFileIgnoreFilterElement(xmlDocument);
            settingsElement.AppendChild(generatedFileFiltersElement);

            foreach (var ignoredFileName in newFileNames)
            {
                var ignoredFileElement = xmlDocument.CreateElement("Value");
                ignoredFileElement.InnerText = AddRegexToFileNameForGeneratedFileFilter(ignoredFileName);
                ignoredFileElement.IsEmpty = false;
                generatedFileFiltersElement.AppendChild(ignoredFileElement);
            }

            return true;
        }

        private static XmlElement BuildNewSourceFileListIgnoreSettingElement(XmlDocument xmlDocument)
        {
            //// Needed XML:
            ////    <Settings>
            ////      <GlobalSettings>
            ////        <BooleanProperty Name = "RulesEnabledByDefault">False</BooleanProperty>
            ////      </GlobalSettings>
            ////    </Settings>

            var settingsElement = xmlDocument.CreateElement("Settings");

            var globalSettingsElement = xmlDocument.CreateElement("GlobalSettings");
            settingsElement.AppendChild(globalSettingsElement);

            var rulesEnabledElement = xmlDocument.CreateElement("BooleanProperty");
            globalSettingsElement.AppendChild(rulesEnabledElement);

            rulesEnabledElement.InnerText = "False";
            var attribute = xmlDocument.CreateAttribute("Name");
            attribute.Value = "RulesEnabledByDefault";
            rulesEnabledElement.Attributes.Append(attribute);

            return settingsElement;
        }

        private static XmlElement BuildNewGeneratedFileIgnoreFilterElement(XmlDocument xmlDocument)
        {
            //// Needed XML:
            ////    <CollectionProperty Name="GeneratedFileFilters">
            ////      <Value>ignoreme.cs</Value>
            ////    </CollectionProperty>

            var collectionPropertyElement = xmlDocument.CreateElement("CollectionProperty");

            var attribute = xmlDocument.CreateAttribute("Name");
            attribute.Value = "GeneratedFileFilters";
            collectionPropertyElement.Attributes.Append(attribute);

            return collectionPropertyElement;
        }

        private static string AddRegexToFileNameForGeneratedFileFilter(string fileName)
        {
            return regexPrefixForGeneratedFileFilterNames + fileName;
        }

        private static string StripRegexFromFileNameForGeneratedFileFilter(string fileNameWithRegex)
        {
            return fileNameWithRegex.Replace(regexPrefixForGeneratedFileFilterNames, string.Empty);
        }
    }
}