/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace RedBlueGames.Tools
{
    using System.Collections;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Bulk renamer handles configuration and renaming of names.
    /// </summary>
    public class BulkRenamer
    {
        private const string AddedTextColorTag = "<color=green>";
        private const string DeletedTextColorTag = "<color=red>";
        private const string EndColorTag = "</color>";

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.Tools.BulkRenamer"/> class.
        /// </summary>
        public BulkRenamer()
        {
            this.Prefix = string.Empty;
            this.Suffix = string.Empty;
            this.SearchString = string.Empty;
            this.ReplacementString = string.Empty;
        }

        /// <summary>
        /// Gets or sets the prefix to add.
        /// </summary>
        /// <value>The prefix to add..</value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix to add.
        /// </summary>
        /// <value>The suffix to add.</value>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the search string, used to determine what text to replace.
        /// </summary>
        /// <value>The search string.</value>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets the replacement string, which replaces instances of the search token.
        /// </summary>
        /// <value>The replacement string.</value>
        public string ReplacementString { get; set; }

        /// <summary>
        /// Gets or sets the number of characters to delete from the front.
        /// </summary>
        /// <value>The number front delete chars.</value>
        public int NumFrontDeleteChars { get; set; }

        /// <summary>
        /// Gets or sets the number of characters to delete from the back.
        /// </summary>
        /// <value>The number back delete chars.</value>
        public int NumBackDeleteChars { get; set; }

        /// <summary>
        /// Gets or sets the starting count.
        /// </summary>
        /// <value>The starting count.</value>
        public int StartingCount { get; set; }

        /// <summary>
        /// Gets or sets the format for the count, appended to the end of the string.
        /// </summary>
        /// <value>The count format.</value>
        public string CountFormat { get; set; }

        private string ColoredReplacementString
        {
            get
            {
                return string.Concat(
                    ColorStringForDelete(this.SearchString),
                    ColorStringForAdd(this.ReplacementString));
            }
        }

        /// <summary>
        /// Gets the string, renamed according to the BulkRenamer configuration.
        /// </summary>
        /// <returns>The renamed strings.</returns>
        /// <param name="includeDiff">If set to <c>true</c> outputs the name including diff with rich text tags.</param>
        /// <param name="showProgress">If set to <c>true</c> show progress with a GUI.</param>
        /// <param name="originalNames">Original names to rename.</param>
        public string[] GetRenamedStrings(bool includeDiff, bool showProgress, params string[] originalNames)
        {
            var renamedStrings = new string[originalNames.Length];
            for (int i = 0; i < originalNames.Length; ++i)
            {
                var count = this.StartingCount + i;

                if (showProgress)
                {
                    var infoString = string.Format(
                                         "Renaming asset {0} of {1}",
                                         i,
                                         originalNames.Length);

                    EditorUtility.DisplayProgressBar(
                        "Renaming Assets...",
                        infoString,
                        i / (float)originalNames.Length);
                }

                renamedStrings[i] = this.GetRenamedString(originalNames[i], count, includeDiff);
            }

            if (showProgress)
            {
                EditorUtility.ClearProgressBar();
            }

            return renamedStrings;
        }

        private static string ColorStringForDelete(string baseString)
        {
            return string.Concat(DeletedTextColorTag, baseString, EndColorTag);
        }

        private static string ColorStringForAdd(string baseString)
        {
            return string.Concat(AddedTextColorTag, baseString, EndColorTag);
        }

        private string GetRenamedString(string originalName, int count, bool includeDiff)
        {
            var modifiedName = originalName;

            // Trim Front chars
            string trimmedFrontChars = string.Empty;
            if (this.NumFrontDeleteChars > 0)
            {
                var numCharsToDelete = Mathf.Min(this.NumFrontDeleteChars, originalName.Length);
                trimmedFrontChars = originalName.Substring(0, numCharsToDelete);

                modifiedName = modifiedName.Remove(0, numCharsToDelete);
            }

            // Trim Back chars
            string trimmedBackChars = string.Empty;
            if (this.NumBackDeleteChars > 0)
            {
                var numCharsToDelete = Mathf.Min(this.NumBackDeleteChars, modifiedName.Length);
                int startIndex = modifiedName.Length - numCharsToDelete;
                trimmedBackChars = modifiedName.Substring(startIndex, numCharsToDelete);

                modifiedName = modifiedName.Remove(startIndex, numCharsToDelete);
            }

            // When showing rich text, add back in the trimmed characters
            if (includeDiff)
            {
                if (!string.IsNullOrEmpty(trimmedFrontChars))
                {
                    modifiedName = string.Concat(ColorStringForDelete(trimmedFrontChars), modifiedName);
                }

                if (!string.IsNullOrEmpty(trimmedBackChars))
                {
                    modifiedName = string.Concat(modifiedName, ColorStringForDelete(trimmedBackChars));
                }
            }

            // Replace strings first so we don't replace the prefix.
            if (!string.IsNullOrEmpty(this.SearchString))
            {
                var replacementString = includeDiff ? this.ColoredReplacementString :
                this.ReplacementString;
                modifiedName = modifiedName.Replace(this.SearchString, replacementString);
            }

            if (!string.IsNullOrEmpty(this.Prefix))
            {
                var prefix = includeDiff ? ColorStringForAdd(this.Prefix) : this.Prefix;
                modifiedName = string.Concat(prefix, modifiedName);
            }

            if (!string.IsNullOrEmpty(this.Suffix))
            {
                var suffix = includeDiff ? ColorStringForAdd(this.Suffix) : this.Suffix;
                modifiedName = string.Concat(modifiedName, suffix);
            }

            if (!string.IsNullOrEmpty(this.CountFormat))
            {
                try
                {
                    var countAsString = count.ToString(this.CountFormat);

                    if (includeDiff)
                    {
                        countAsString = string.Concat(ColorStringForAdd(countAsString));
                    }

                    modifiedName = string.Concat(modifiedName, countAsString);
                }
                catch (System.FormatException)
                {
                    // Can't append anything if format is bad.
                }
            }

            return modifiedName;
        }
    }
}