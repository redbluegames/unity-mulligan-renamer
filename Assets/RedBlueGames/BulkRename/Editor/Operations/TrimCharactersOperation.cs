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
    /// RenameOperation used to trim characters from the front or back of the rename string.
    /// </summary>
    public class TrimCharactersOperation : BaseRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.TrimCharactersOperation"/> class.
        /// </summary>
        public TrimCharactersOperation()
        {
            this.NumFrontDeleteChars = 0;
            this.NumBackDeleteChars = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.TrimCharactersOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public TrimCharactersOperation(TrimCharactersOperation operationToCopy)
        {
            this.NumFrontDeleteChars = operationToCopy.NumFrontDeleteChars;
            this.NumBackDeleteChars = operationToCopy.NumBackDeleteChars;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Trim Characters";
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
                return 3;
            }
        }

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
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new TrimCharactersOperation(this);
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
            var modifiedName = input;

            // Trim Front chars
            string trimmedFrontChars = string.Empty;
            if (this.NumFrontDeleteChars > 0)
            {
                var numCharsToDelete = Mathf.Min(this.NumFrontDeleteChars, input.Length);
                trimmedFrontChars = input.Substring(0, numCharsToDelete);

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
                    modifiedName = string.Concat(BaseRenameOperation.ColorStringForDelete(trimmedFrontChars), modifiedName);
                }

                if (!string.IsNullOrEmpty(trimmedBackChars))
                {
                    modifiedName = string.Concat(modifiedName, BaseRenameOperation.ColorStringForDelete(trimmedBackChars));
                }
            }

            return modifiedName;
        }

        /// <summary>
        /// Draws the element as a GUI using EditorGUILayout calls. This should return a copy of the 
        /// Operation with the modified data. This way we mirror how regular GUI calls work.
        /// </summary>
        /// <returns>A modified copy of the Operation.</returns>
        public override BaseRenameOperation DrawGUI()
        {   
            var clone = new TrimCharactersOperation(this);
            EditorGUILayout.LabelField("Trimming", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            clone.NumFrontDeleteChars = EditorGUILayout.IntField("Delete From Front", this.NumFrontDeleteChars);
            clone.NumFrontDeleteChars = Mathf.Max(0, clone.NumFrontDeleteChars);
            clone.NumBackDeleteChars = EditorGUILayout.IntField("Delete from Back", this.NumBackDeleteChars);
            clone.NumBackDeleteChars = Mathf.Max(0, clone.NumBackDeleteChars);
            EditorGUI.indentLevel--;
            return clone;
        }
    }
}