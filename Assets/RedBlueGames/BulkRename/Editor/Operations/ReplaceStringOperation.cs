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
    using System.Collections;
    using System.Collections.Generic;
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
            this.RegexSearchString = string.Empty;
            this.RegexReplacementString = string.Empty;
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
            this.RegexSearchString = operationToCopy.RegexSearchString;
            this.RegexReplacementString = operationToCopy.RegexReplacementString;
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
        /// Gets or sets the regex search string.
        /// </summary>
        /// <value>The regex search string.</value>
        public string RegexSearchString { get; set; }

        /// <summary>
        /// Gets or sets the regex replacement string.
        /// </summary>
        /// <value>The regex replacement string.</value>
        public string RegexReplacementString { get; set; }

        /// <summary>
        /// Gets or sets the search string, used to determine what text to replace.
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
                    return !IsValidRegex(this.RegexReplacementString)
                    || !IsValidRegex(this.RegexSearchString);
                }
                else
                {
                    return false;
                }
            }
        }

        private string ActiveSearchPattern
        {
            get
            {
                if (this.UseRegex)
                {
                    return this.RegexSearchString;
                }
                else
                {
                    string searchStringRegexPattern = string.Empty;

                    if (!string.IsNullOrEmpty(this.SearchString))
                    {
                        // Create capture group regex to extract the matched string
                        // Escape the non-regex search string to prevent any embedded patterns from being interpretted as regex.
                        searchStringRegexPattern = string.Concat("(", Regex.Escape(this.SearchString), ")");
                    }

                    return searchStringRegexPattern;
                }
            }
        }

        private string ActiveReplacementPattern
        {
            get
            {
                if (this.UseRegex)
                {
                    return this.RegexReplacementString;
                }
                else
                {
                    return this.ReplacementString;
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
        /// Rename the specified input, using the relativeCount. Optionally output the string as a diff.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <param name="includeDiff">If set to <c>true</c> include diff.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override string Rename(string input, int relativeCount, bool includeDiff)
        {
            if (!string.IsNullOrEmpty(this.ActiveSearchPattern))
            {
                var regexOptions = this.SearchIsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;

                var replacement = string.Empty;
                if (includeDiff)
                {
                    replacement = BaseRenameOperation.ColorStringForDelete("$&");

                    // If replacement pattern is empty is screws up the green color tags on the diff.
                    if (!string.IsNullOrEmpty(this.ActiveReplacementPattern))
                    {
                        replacement += BaseRenameOperation.ColorStringForAdd(this.ActiveReplacementPattern);
                    }
                }
                else
                {
                    replacement = this.ActiveReplacementPattern;
                }

                // Regex gives us two features - case insensitivity and capture groups. Capture groups
                // are used so that the diff shows the term that is replaced, not the term that replaces it.
                // (ex. Char_Herohero searching for hero replacing with 'z' should show "Char_Herozheroz" not 
                // "Char_herozheroz".
                try
                {
                    return Regex.Replace(input, this.ActiveSearchPattern, replacement, regexOptions);
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
        /// Draws the element as a GUI using EditorGUILayout calls. This should return a copy of the 
        /// Operation with the modified data. This way we mirror how regular GUI calls work.
        /// </summary>
        /// <returns>A modified copy of the Operation.</returns>
        public override BaseRenameOperation DrawGUI()
        {   
            var clone = new ReplaceStringOperation(this);
            EditorGUILayout.LabelField("Text Replacement", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var regexToggleContent = new GUIContent("Use Regular Expression", "Match terms using Regular Expressions, terms that allow for powerful pattern matching.");
            clone.UseRegex = EditorGUILayout.Toggle(regexToggleContent, this.UseRegex);

            if (clone.UseRegex)
            {
                var regexSearchContent = new GUIContent("Match Expression", "Regular Expression to use to match terms.");
                clone.RegexSearchString = EditorGUILayout.TextField(regexSearchContent, this.RegexSearchString);
                var regexReplacmentContent = new GUIContent("Replacement Expression", "Regular Expression to use when replacing matched patterns.");
                clone.RegexReplacementString = EditorGUILayout.TextField(regexReplacmentContent, this.RegexReplacementString);
            }
            else
            {
                var searchStringContent = new GUIContent(
                                              "Search for String", 
                                              "Substrings to search for in the filenames. These strings will be replaced by the Replacement String.");
                clone.SearchString = EditorGUILayout.TextField(searchStringContent, this.SearchString);

                var replacementStringContent = new GUIContent(
                                                   "Replace with", 
                                                   "String to replace matching instances of the Search string.");
                clone.ReplacementString = EditorGUILayout.TextField(replacementStringContent, this.ReplacementString);
            }

            var caseSensitiveContent = new GUIContent(
                                           "Case Sensitive", 
                                           "Search using case sensitivity. Only strings that match the supplied casing will be replaced.");
            clone.SearchIsCaseSensitive = EditorGUILayout.Toggle(caseSensitiveContent, this.SearchIsCaseSensitive);

            if (this.HasErrors)
            {
                if (!IsValidRegex(this.ActiveSearchPattern))
                {
                    EditorGUILayout.HelpBox(
                        "Match Expression is not a valid Regular Expression.",
                        MessageType.Error);
                }

                if (!IsValidRegex(this.RegexReplacementString))
                {
                    EditorGUILayout.HelpBox(
                        "Replacement Expression is not a valid Regular Expression.",
                        MessageType.Error);
                }
            }

            EditorGUI.indentLevel--;
            return clone;
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