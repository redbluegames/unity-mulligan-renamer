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

namespace RedBlueGames.BulkRename
{
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class ReplaceStringOperation : BaseRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ReplaceStringOperation"/> class.
        /// </summary>
        public ReplaceStringOperation()
        {
            this.UseRegex = false;
            this.SearchString = string.Empty;
            this.SearchIsCaseSensitive = false;
            this.ReplacementString = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ReplaceStringOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ReplaceStringOperation(ReplaceStringOperation operationToCopy)
        {
            this.UseRegex = operationToCopy.UseRegex;
            this.SearchString = operationToCopy.SearchString;
            this.SearchIsCaseSensitive = operationToCopy.SearchIsCaseSensitive;
            this.ReplacementString = operationToCopy.ReplacementString;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Replace String";
            }
        }

        /// <summary>
        /// Gets the order in which this rename op is displayed in the Add Op menu (lower is higher in the list.)
        /// </summary>
        /// <value>The menu order.</value>
        public override int MenuOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RedBlueGames.BulkRename.ReplaceStringOperation"/>
        /// uses a regex expression for input.
        /// </summary>
        /// <value><c>true</c> if input is a regular expression; otherwise, <c>false</c>.</value>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets the search string that will be replaced.
        /// </summary>
        /// <value>The search string.</value>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case sensitive.
        /// </summary>
        /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
        public bool SearchIsCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the replacement string, which replaces instances of the search token.
        /// </summary>
        /// <value>The replacement string.</value>
        public string ReplacementString { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has errors that prevent it from Renaming.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public override bool HasErrors
        {
            get
            {
                if (this.UseRegex)
                {
                    return !IsValidRegex(this.ReplacementString)
                    || !IsValidRegex(this.SearchString);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Replace String";
            }
        }

        private string SearchRegexPattern
        {
            get
            {
                if (this.UseRegex)
                {
                    return this.SearchString;
                }
                else
                {
                    string searchStringRegexPattern = string.Empty;

                    if (!string.IsNullOrEmpty(this.SearchString))
                    {
                        // Escape the non-regex search string to prevent any embedded patterns from being interpretted as regex.
                        searchStringRegexPattern = string.Concat(Regex.Escape(this.SearchString));
                    }

                    return searchStringRegexPattern;
                }
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new ReplaceStringOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override string Rename(string input, int relativeCount)
        {
            if (!string.IsNullOrEmpty(this.SearchRegexPattern))
            {
                var regexOptions = this.SearchIsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                var replacement = this.ReplacementString;

                try
                {
                    // Regex gives us case sensitivity, even when not searching with regex.
                    return Regex.Replace(input, this.SearchRegexPattern, replacement, regexOptions);
                }
                catch (System.ArgumentException)
                {
                    return input;
                }
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected override void DrawContents()
        {   
            var regexToggleContent = new GUIContent("Use Regular Expression", "Match terms using Regular Expressions, terms that allow for powerful pattern matching.");
            this.UseRegex = EditorGUILayout.Toggle(regexToggleContent, this.UseRegex);

            GUIContent searchContent;
            GUIContent replacementContent;
            if (this.UseRegex)
            {
                searchContent = new GUIContent("Match Regex", "Regular Expression to use to match terms.");
                replacementContent = new GUIContent("Replacement Regex", "Regular Expression to use when replacing matched patterns.");
            }
            else
            {
                searchContent = new GUIContent(
                    "Search for String", 
                    "Substrings to search for in the filenames. These strings will be replaced by the Replacement String.");
                replacementContent = new GUIContent(
                    "Replace with", 
                    "String to replace matching instances of the Search string.");
            }

            this.SearchString = EditorGUILayout.TextField(searchContent, this.SearchString);
            this.ReplacementString = EditorGUILayout.TextField(replacementContent, this.ReplacementString);

            var caseSensitiveContent = new GUIContent(
                                           "Case Sensitive", 
                                           "Search using case sensitivity. Only strings that match the supplied casing will be replaced.");
            this.SearchIsCaseSensitive = EditorGUILayout.Toggle(caseSensitiveContent, this.SearchIsCaseSensitive);

            if (this.HasErrors)
            {
                if (!IsValidRegex(this.SearchRegexPattern))
                {
                    EditorGUILayout.HelpBox(
                        "Match Expression is not a valid Regular Expression.",
                        MessageType.Error);
                }

                if (!IsValidRegex(this.ReplacementString))
                {
                    EditorGUILayout.HelpBox(
                        "Replacement Expression is not a valid Regular Expression.",
                        MessageType.Error);
                }
            }
        }

        private static bool IsValidRegex(string pattern)
        {
            // We consider empty a valid regular expression since Rename handles it gracefully
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            try
            {
                Regex.Match(string.Empty, pattern);
            }
            catch (System.ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}