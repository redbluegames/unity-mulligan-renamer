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
    [System.Serializable]
    public class ChangeCaseOperation : IRenameOperation
    {
        [SerializeField]
        private CasingChange casing;

        [SerializeField]
        private bool changeFirstCharacterOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class.
        /// </summary>
        public ChangeCaseOperation()
        {
            this.Casing = CasingChange.Lowercase;
            this.changeFirstCharacterOnly = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCaseOperation"/> class by copying another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ChangeCaseOperation(ChangeCaseOperation operationToCopy)
        {
            this.Casing = operationToCopy.Casing;
            this.changeFirstCharacterOnly = operationToCopy.changeFirstCharacterOnly;
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
        public CasingChange Casing
        {
            get
            {
                return this.casing;
            }

            set
            {
                this.casing = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether or not to change all characters or just the first.
        /// </summary>
        public bool ChangeFirstCharacterOnly
        {
            get
            {
                return this.changeFirstCharacterOnly;
            }

            set
            {
                this.changeFirstCharacterOnly = value;
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
            var clone = new ChangeCaseOperation(this);
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

            if (this.ChangeFirstCharacterOnly)
            {
                var firstCharacter = inputCaseChanged.Substring(0, 1);
                var remainingCharacters = string.Empty;
                if (inputCaseChanged.Length > 1)
                {
                    remainingCharacters = input.Substring(1, input.Length - 1);
                }

                inputCaseChanged = string.Concat(firstCharacter, remainingCharacters);
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

        /// <summary>
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            return this.Casing.GetHashCode();
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as ChangeCaseOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.Casing != otherAsOp.Casing)
            {
                return false;
            }

            return true;
        }
    }
}