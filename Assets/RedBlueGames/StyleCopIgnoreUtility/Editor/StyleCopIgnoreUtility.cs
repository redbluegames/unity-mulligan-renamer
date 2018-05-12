namespace RedBlueGames.StyleCopIgnoreUtility
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility that manages ignored source files for StyleCop and interfaces with the StyleCop .Settings file
    /// </summary>
    internal class StyleCopIgnoreUtility
    {
        //// ====================================================================================================================

        internal const string UtilityName = "StyleCopIgnoreUtility";
        internal const string WindowName = "StyleCop";
        internal const string StyleCopFileExtension = "StyleCop";

        internal const string UtilitySettingsFileName = "StyleCopIgnoreUtilitySettings.asset";
        internal static readonly string UtilitySettingsAbsolutePath = Application.dataPath + "/Resources/RedBlueGames";
        internal static readonly string UtilitySettingsRelativePath = "Assets" + "/Resources/RedBlueGames/";

        //// ====================================================================================================================

        private Preferences userPreferences;
        private StyleCopIgnoreUtilityData saveData;
        private IStyleCopIgnoreUtilityView view;

        private bool cachedIsStyleCopSettingsFileValid;
        private bool cachedIsStyleCopSynced;

        //// ====================================================================================================================

        private StyleCopIgnoreUtility()
        {
        }

        //// ====================================================================================================================

        /// <summary>
        /// Gets the user preferences for the utility GUI
        /// </summary>
        internal Preferences UserPreferences
        {
            get
            {
                return this.userPreferences;
            }
        }

        /// <summary>
        /// Gets the Save Data (Unity .asset) for the utility
        /// </summary>
        internal StyleCopIgnoreUtilityData SaveData
        {
            get
            {
                return this.saveData;
            }
        }

        /// <summary>
        /// Gets the utility's View window
        /// </summary>
        internal IStyleCopIgnoreUtilityView View
        {
            get
            {
                return this.view;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the StyleCop .Settings file is valid
        /// </summary>
        internal bool CachedIsStyleCopSettingsFileValid
        {
            get
            {
                return this.cachedIsStyleCopSettingsFileValid;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the StyleCop .Settings file is sync'd with the SaveData
        /// </summary>
        internal bool CachedIsStyleCopSynced
        {
            get
            {
                return this.cachedIsStyleCopSynced;
            }
        }

        //// ====================================================================================================================

        /// <summary>
        /// Creates an instance of the StyleCopIgnoreUtility to be represented by the supplied View
        /// </summary>
        /// <param name="view">The View that should represent the utility</param>
        public static void CreateWithView(IStyleCopIgnoreUtilityView view)
        {
            var utility = new StyleCopIgnoreUtility();

            utility.view = view;
            view.SetUtility(utility);

            utility.Initialize();
        }

        /// <summary>
        /// Sets the location of the StyleCop .Settings XML file, and validates it
        /// </summary>
        /// <param name="filePath">UniversalPath to the StyleCop .Settings file</param>
        internal void SetStyleCopFile(UniversalPath filePath)
        {
            string errorMessage;
            if (!XMLTool.IsStyleCopFileValid(filePath, out errorMessage))
            {
                throw new System.ArgumentException(errorMessage);
            }

            this.saveData.StyleCopSettingsFilePath = filePath;

            this.cachedIsStyleCopSettingsFileValid = true;
            this.CheckStyleCopFileSynced();
        }

        /// <summary>
        /// Checks and caches whether the current StyleCop .Settings file is valid
        /// </summary>
        internal void CheckStyleCopFileValidity()
        {
            this.cachedIsStyleCopSettingsFileValid = XMLTool.IsStyleCopFileValid(this.saveData.StyleCopSettingsFilePath);
        }

        /// <summary>
        /// Checks and caches whether the StyleCop .Settings file is sync'd with the SaveData
        /// </summary>
        internal void CheckStyleCopFileSynced()
        {
            if (this.CachedIsStyleCopSettingsFileValid)
            {
                this.cachedIsStyleCopSynced = this.SaveData.IsIgnoredFilesEqualToFileNameList(XMLTool.GetIgnoredFileNames(this.SaveData.StyleCopSettingsFilePath));
            }
        }

        /// <summary>
        /// Saves the user's current selection of ignored files/directories to the SaveData
        /// </summary>
        internal void OnSave()
        {
            var allSelectedIgnoredFiles = this.view.GetSelectedFiles();

            this.saveData.IgnoredFilePaths = allSelectedIgnoredFiles.ToArray();
            EditorUtility.SetDirty(this.saveData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            this.CheckStyleCopFileSynced();
        }

        /// <summary>
        /// Applies the user's current selection of ignored files/directories to StyleCop's .Settings file
        /// </summary>
        internal void OnLoadToStyleCop()
        {
            var allSelectedIgnoredFiles = this.view.GetSelectedFiles().ToArray();

            // Convert full file paths to just file names
            var ignoredFileNames = new string[allSelectedIgnoredFiles.Length];
            for (int i = 0; i < allSelectedIgnoredFiles.Length; ++i)
            {
                ignoredFileNames[i] = new System.IO.FileInfo((string)allSelectedIgnoredFiles[i]).Name;
            }

            XMLTool.ReplaceIgnoreFileNames(this.saveData.StyleCopSettingsFilePath, ignoredFileNames);

            this.CheckStyleCopFileSynced();
        }

        /// <summary>
        /// Removes all ignores filenames from StyleCop's .Settings file, regardless of their ignore status in the View
        /// </summary>
        internal void OnUnloadFromStyleCop()
        {
            XMLTool.ReplaceIgnoreFileNames(this.saveData.StyleCopSettingsFilePath, new string[0]);

            this.CheckStyleCopFileSynced();
        }

        /// <summary>
        /// Quits the Utility
        /// </summary>
        internal void OnQuit()
        {
            this.view.WritePreferences(this.userPreferences);
            this.userPreferences.Save();
        }

        private void Initialize()
        {
            this.LoadUserPreferences();
            this.LoadSavedData();
            this.view.InitializeWithData(this.SaveData);

            this.CheckStyleCopFileValidity();
            this.CheckStyleCopFileSynced();
        }

        private void LoadUserPreferences()
        {
            this.userPreferences = new Preferences();
            this.userPreferences.Load();

            this.view.ReadPreferences(this.userPreferences);
        }

        private void LoadSavedData()
        {
            var folder = Application.dataPath + "/Resources";
            if (!System.IO.Directory.Exists(folder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            folder = UtilitySettingsAbsolutePath;
            if (!System.IO.Directory.Exists(folder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "RedBlueGames");
            }

            this.saveData = AssetDatabase.LoadAssetAtPath<StyleCopIgnoreUtilityData>(UtilitySettingsRelativePath + UtilitySettingsFileName);
            if (this.SaveData == null)
            {
                var newModelAsset = ScriptableObject.CreateInstance<StyleCopIgnoreUtilityData>();
                try
                {
                    AssetDatabase.CreateAsset(newModelAsset, UtilitySettingsRelativePath + UtilitySettingsFileName);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                this.saveData = AssetDatabase.LoadAssetAtPath<StyleCopIgnoreUtilityData>(UtilitySettingsRelativePath + UtilitySettingsFileName);
                if (this.SaveData == null)
                {
                    // Uhh, wut.
                    throw new System.InvalidOperationException("Failed to create StyleCopIgnoreUtility asset!");
                }
            }
        }
    }
}