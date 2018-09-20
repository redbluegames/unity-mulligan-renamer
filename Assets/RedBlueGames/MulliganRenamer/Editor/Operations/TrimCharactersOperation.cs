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
    public class TrimCharactersOperation : IRenameOperation
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

        [SerializeField]
        private int numFrontDeleteChars;

        [SerializeField]
        private int numBackDeleteChars;

        /// <summary>
        /// Gets or sets the number of characters to delete from the front.
        /// </summary>
        /// <value>The number front delete chars.</value>
        public int NumFrontDeleteChars
        {
            get
            {
                return this.numFrontDeleteChars;
            }

            set
            {
                this.numFrontDeleteChars = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of characters to delete from the back.
        /// </summary>
        /// <value>The number back delete chars.</value>
        public int NumBackDeleteChars
        {
            get
            {
                return this.numBackDeleteChars;
            }

            set
            {
                this.numBackDeleteChars = value;
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
            var clone = new TrimCharactersOperation(this);
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
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            // Easy hash method:
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            int hash = 17;
            hash = hash * 23 + this.NumFrontDeleteChars.GetHashCode();
            hash = hash * 23 + this.NumBackDeleteChars.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as TrimCharactersOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.NumFrontDeleteChars != otherAsOp.NumFrontDeleteChars)
            {
                return false;
            }

            if (this.NumBackDeleteChars != otherAsOp.NumBackDeleteChars)
            {
                return false;
            }

            return true;
        }
    }
}