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

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to delete all characters in a name and replace it with an entirely new one.
    /// </summary>
    public class ReplaceNameOperation : RenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceNameOperation"/> class.
        /// </summary>
        public ReplaceNameOperation()
        {
            this.NewName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceNameOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ReplaceNameOperation(ReplaceNameOperation operationToCopy)
        {
            this.NewName = operationToCopy.NewName;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Replace/Rename";
            }
        }

        /// <summary>
        /// Gets or sets the new name.
        /// </summary>
        /// <value>The new name.</value>
        public string NewName { get; set; }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return "Rename";
            }
        }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public override Color32 HighlightColor
        {
            get
            {
                return this.ReplaceColor;
            }
        }

        /// <summary>
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public override string ControlToFocus
        {
            get
            {
                return "New Name";
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override RenameOperation Clone()
        {
            var clone = new ReplaceNameOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override RenameResult Rename(string input, int relativeCount)
        {
            var renameResult = new RenameResult();

            if (!string.IsNullOrEmpty(input))
            {
                renameResult.Add(new Diff(input, DiffOperation.Deletion));
            }

            if (!string.IsNullOrEmpty(this.NewName))
            {
                renameResult.Add(new Diff(this.NewName, DiffOperation.Insertion));
            }

            return renameResult;
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(1);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            // Split even a single line GUI so that we properly subtract out spacing
            var singleLineRect = operationRect.GetSplitVertical(1, 1, RenameOperation.LineSpacing);

            GUIContent newNameContent = new GUIContent("New Name", "Name to replace the old one with.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, newNameContent.text));
            this.NewName = EditorGUI.TextField(singleLineRect, newNameContent, this.NewName);
        }
    }
}