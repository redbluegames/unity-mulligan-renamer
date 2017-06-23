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
    /// RenameOperation that changes the case of the characters in the name.
    /// </summary>
    public class ChangeCaseOperation : BaseRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ChangeCaseOperation"/> class.
        /// </summary>
        public ChangeCaseOperation()
        {
            this.ToUpper = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ChangeCaseOperation"/> class by copying another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ChangeCaseOperation(ChangeCaseOperation operationToCopy)
        {
            this.ToUpper = operationToCopy.ToUpper;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Modify/Change Case";
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
                return 4;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RedBlueGames.BulkRename.ChangeCaseOperation"/> changes the case to uppercase.
        /// </summary>
        /// <value><c>true</c> if to upper; otherwise, <c>false</c>.</value>
        public bool ToUpper { get; set; }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Change Case";
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new ChangeCaseOperation(this);
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
            if (this.ToUpper)
            {
                return input.ToUpper();
            }
            else
            {
                return input.ToLower();
            }
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected override void DrawContents()
        {   
            this.ToUpper = EditorGUILayout.Toggle("To Uppercase", this.ToUpper);
        }
    }
}