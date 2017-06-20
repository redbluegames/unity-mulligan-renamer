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
    /// RenameOperation that removes specific characters from the names.
    /// </summary>
    public class RemoveCharactersOperation : BaseRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.RemoveCharactersOperation"/> class.
        /// </summary>
        public RemoveCharactersOperation()
        {
            this.Characters = string.Empty;
            this.IsCaseSensitive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.RemoveCharactersOperation"/> class.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public RemoveCharactersOperation(RemoveCharactersOperation operationToCopy)
        {
            this.Characters = operationToCopy.Characters;
            this.IsCaseSensitive = operationToCopy.IsCaseSensitive;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Remove Characters";
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
                return 6;
            }
        }

        /// <summary>
        /// Gets or sets the characters to remove.
        /// </summary>
        /// <value>The characters.</value>
        public string Characters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance searches for the characters using case sensitivity.
        /// </summary>
        /// <value><c>true</c> if the search is case sensitive; otherwise, <c>false</c>.</value>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Remove Characters";
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new RemoveCharactersOperation(this);
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
            if (!string.IsNullOrEmpty(this.Characters))
            {
                var regexOptions = this.IsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                var replacement = string.Empty;

                try
                {
                    var regexPattern = Regex.Escape(this.Characters);
                    var charactersAsRegex = string.Concat("[", regexPattern, "]");
                    return Regex.Replace(input, charactersAsRegex, replacement, regexOptions);
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
            var charactersFieldContent = new GUIContent("Characters to Remove", "All characters that will be removed from the names.");
            this.Characters = EditorGUILayout.TextField(charactersFieldContent, this.Characters);

            var caseSensitiveToggleContent = new GUIContent("Case Sensitive", "Flag the search to match only the specified case");
            this.IsCaseSensitive = EditorGUILayout.Toggle(caseSensitiveToggleContent, this.IsCaseSensitive);
        }
    }
}