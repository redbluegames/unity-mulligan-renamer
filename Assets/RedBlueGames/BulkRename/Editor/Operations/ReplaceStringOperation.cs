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
            if (!string.IsNullOrEmpty(this.SearchString))
            {
                // Create capture group regex to extract the matched string
                // Escape the non-regex search string to prevent it from being interpretted as regex.
                var searchStringRegexPattern = string.Concat("(", Regex.Escape(this.SearchString), ")");
                var regexOptions = this.SearchIsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;

                var replacement = string.Empty;
                if (includeDiff)
                {
                    replacement = BaseRenameOperation.ColorStringForDelete("$1");
                    replacement += BaseRenameOperation.ColorStringForAdd(this.ReplacementString);
                }
                else
                {
                    replacement = this.ReplacementString;
                }

                // Regex gives us two features - case insensitivity and capture groups. Capture groups
                // are used so that the diff shows the term that is replaced, not the term that replaces it.
                // (ex. Char_Herohero searching for hero replacing with 'z' should show "Char_Herozheroz" not 
                // "Char_herozheroz".
                return Regex.Replace(input, searchStringRegexPattern, replacement, regexOptions);
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
            clone.SearchString = EditorGUILayout.TextField("Search for String", this.SearchString);
            clone.ReplacementString = EditorGUILayout.TextField("Replace with", this.ReplacementString);
            clone.SearchIsCaseSensitive = EditorGUILayout.Toggle("Case Sensitive", this.SearchIsCaseSensitive);
            EditorGUI.indentLevel--;
            return clone;
        }
    }
}