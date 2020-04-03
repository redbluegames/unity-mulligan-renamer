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
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation that changes the case of the characters in the name.
    /// </summary>
    [System.Serializable]
    public class AdjustNumberingOperation : IRenameOperation
    {
        [SerializeField]
        private int offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class.
        /// </summary>
        public AdjustNumberingOperation()
        {
            this.offset = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class by copying another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public AdjustNumberingOperation(AdjustNumberingOperation operationToCopy)
        {
            this.Offset = operationToCopy.Offset;
        }

        public int Offset
        {
            get
            {
                return this.offset;
            }

            set
            {
                this.offset = value;
            }
        }

        public bool HasErrors()
        {
            return false;
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public IRenameOperation Clone()
        {
            var clone = new AdjustNumberingOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public RenameResult Rename(string input, int relativeCount)
        {
            if (string.IsNullOrEmpty(input))
            {
                return RenameResult.Empty;
            }

            var parseNumbersRegex = "([0-9]+)";

            var regex = new Regex(parseNumbersRegex);
            var matches = regex.Matches(input);

            var fullReplacementResult = RenameResultUtilities.CreateDiffFromReplacedMatches(input, this.ReplaceMatch, matches);
            var renameResult = RenameResultUtilities.GetDiffResultFromStrings(input, fullReplacementResult.Result);
            return renameResult;
        }

        private string ReplaceMatch(Match match)
        {
            try
            {
                int parsedInt = int.Parse(match.Value);
                parsedInt += this.offset;
                return parsedInt.ToString();
            }
            catch (System.Exception)
            {
                return match.Value;
            }
        }

        /// <summary>
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            return this.Offset.GetHashCode();
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as AdjustNumberingOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.Offset != otherAsOp.Offset)
            {
                return false;
            }

            return true;
        }
    }
}