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
    /// RenameOperation that changes the case of the characters in the name.
    /// </summary>
    public class ChangeCaseOperation : RenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class.
        /// </summary>
        public ChangeCaseOperation()
        {
            this.Casing = CasingChange.Lowercase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class by copying another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ChangeCaseOperation(ChangeCaseOperation operationToCopy)
        {
            this.Casing = operationToCopy.Casing;
        }

        /// <summary>
        /// Casing change describes all possible new casings.
        /// </summary>
        public enum CasingChange
        {
            /// <summary>
            /// Change all characters to lowercase
            /// </summary>
            Lowercase,

            /// <summary>
            /// Change all character to uppercase
            /// </summary>
            Uppercase
        }

        /// <summary>
        /// Gets or sets the desired casing.
        /// </summary>
        /// <value>The desired casing.</value>
        public CasingChange Casing { get; set; }

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
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return "Change Case";
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
                return this.ModifyColor;
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
                return "To Uppercase";
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override RenameOperation Clone()
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
        public override RenameResult Rename(string input, int relativeCount)
        {
            if (string.IsNullOrEmpty(input))
            {
                return RenameResult.Empty;
            }

            var inputCaseChanged = input;
            switch (this.Casing)
            {
                case CasingChange.Lowercase:
                    inputCaseChanged = input.ToLower();
                    break;
                case CasingChange.Uppercase:
                    inputCaseChanged = input.ToUpper();
                    break;
                default:
                    var message = string.Format(
                                      "CaseOperation received unknown CasingOption {0}",
                                      this.Casing);
                    throw new System.ArgumentOutOfRangeException(message);
            }

            var renameResult = new RenameResult();
            var consecutiveEqualChars = string.Empty;
            for (int i = 0; i < input.Length; ++i)
            {
                string oldLetter = input.Substring(i, 1);
                string newLetter = inputCaseChanged.Substring(i, 1);
                if (oldLetter.Equals(newLetter))
                {
                    consecutiveEqualChars = string.Concat(consecutiveEqualChars, oldLetter);
                }
                else
                {
                    if (!string.IsNullOrEmpty(consecutiveEqualChars))
                    {
                        renameResult.Add(new Diff(consecutiveEqualChars, DiffOperation.Equal));
                        consecutiveEqualChars = string.Empty;
                    }

                    renameResult.Add(new Diff(oldLetter, DiffOperation.Deletion));
                    renameResult.Add(new Diff(newLetter, DiffOperation.Insertion));
                }
            }

            if (!string.IsNullOrEmpty(consecutiveEqualChars))
            {
                renameResult.Add(new Diff(consecutiveEqualChars, DiffOperation.Equal));
            }

            return renameResult;
        }

        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateHeightForGUILines(1);
        }

        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var casingLabel = new GUIContent("New Casing", "The desired casing for the new name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "To Uppercase"));
            this.Casing = (CasingChange)EditorGUI.EnumPopup(operationRect, casingLabel, this.Casing);
        }
    }
}