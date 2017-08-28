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
    /// RenameOperation used to trim characters from the front or back of the rename string.
    /// </summary>
    public class TrimCharactersOperation : RenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrimCharactersOperation"/> class.
        /// </summary>
        public TrimCharactersOperation()
        {
            this.NumFrontDeleteChars = 0;
            this.NumBackDeleteChars = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrimCharactersOperation"/> class.
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
                return "Delete/Trim Characters";
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
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return "Trim Characters";
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
                return this.DeleteColor;
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
                return "Delete from Front";
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override RenameOperation Clone()
        {
            var clone = new TrimCharactersOperation(this);
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

            var numCharactersFromFront = Mathf.Clamp(this.NumFrontDeleteChars, 0, input.Length);
            var numCharactersNotTrimmedFromFront = input.Length - numCharactersFromFront;
            var numCharactersFromBack = Mathf.Clamp(this.NumBackDeleteChars, 0, numCharactersNotTrimmedFromFront);
            var numUntrimmedChars = Mathf.Max(input.Length - (numCharactersFromFront + numCharactersFromBack), 0);

            var result = new RenameResult();
            if (numCharactersFromFront > 0)
            {
                var trimmedSubstring = input.Substring(0, numCharactersFromFront);
                result.Add(new Diff(trimmedSubstring, DiffOperation.Deletion));
            }

            if (numUntrimmedChars > 0)
            {
                var trimmedSubstring = input.Substring(numCharactersFromFront, numUntrimmedChars);
                result.Add(new Diff(trimmedSubstring, DiffOperation.Equal));
            }

            if (numCharactersFromBack > 0)
            {
                var trimmedSubstring = input.Substring(input.Length - numCharactersFromBack, numCharactersFromBack);
                result.Add(new Diff(trimmedSubstring, DiffOperation.Deletion));
            }

            return result;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(int controlPrefix)
        {   
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Delete from Front"));
            this.NumFrontDeleteChars = EditorGUILayout.IntField("Delete from Front", this.NumFrontDeleteChars);
            this.NumFrontDeleteChars = Mathf.Max(0, this.NumFrontDeleteChars);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Delete from Back"));
            this.NumBackDeleteChars = EditorGUILayout.IntField("Delete from Back", this.NumBackDeleteChars);
            this.NumBackDeleteChars = Mathf.Max(0, this.NumBackDeleteChars);
        }
    }
}