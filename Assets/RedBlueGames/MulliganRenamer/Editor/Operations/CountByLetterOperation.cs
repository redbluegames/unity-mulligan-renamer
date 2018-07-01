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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Rename operation that counts using letters of the alphabet.
    /// </summary>
    public class CountByLetterOperation : IRenameOperation
    {
        public static readonly string[] UppercaseAlphabet = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
            "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        public static readonly string[] LowercaseAlphabet = new string[]
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o",
            "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> class.
        /// </summary>
        public CountByLetterOperation()
        {
            this.StartingCount = 0;
            this.Increment = 1;
            this.CountSequence = new string[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public CountByLetterOperation(CountByLetterOperation operationToCopy)
        {
            this.DoNotCarryOver = operationToCopy.DoNotCarryOver;
            this.StartingCount = operationToCopy.StartingCount;
            this.Increment = operationToCopy.Increment;
            this.CountSequence = new string[operationToCopy.CountSequence.Length];
            operationToCopy.CountSequence.CopyTo(this.CountSequence, 0);
        }

        /// <summary>
        /// Gets or sets the count sequence, the sequence of strings to apply in order, corresponding
        /// with the count.
        /// </summary>
        public string[] CountSequence { get; set; }

        /// <summary>
        /// Gets or sets the starting count which offsets all letter assignments.
        /// </summary>
        public int StartingCount { get; set; }

        /// <summary>
        /// Gets or sets the increment to use when counting.
        /// </summary>
        public int Increment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="T:RedBlueGames.MulliganRenamer.CountByLetterOperation"/> should not carry
        /// over the sequence to another "digit".
        /// </summary>
        public bool DoNotCarryOver { get; set; }

        /// <summary>
        /// Checks if this RenameOperation has errors in its configuration.
        /// </summary>
        /// <returns><c>true</c>, if operation has errors, <c>false</c> otherwise.</returns>
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
            var clone = new CountByLetterOperation(this);
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
            var renameResult = new RenameResult();
            if (!string.IsNullOrEmpty(input))
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            var offsetCount = (relativeCount * this.Increment) + this.StartingCount;
            var stringToInsert = this.GetStringFromSequenceForIndex(offsetCount);
            if (!string.IsNullOrEmpty(stringToInsert))
            {
                renameResult.Add(new Diff(stringToInsert, DiffOperation.Insertion));
            }

            return renameResult;
        }

        /// <summary>
        /// Gets the string to use from the sequence based on the index
        /// </summary>
        /// <returns>The string to use for the given index.</returns>
        /// <param name="index">Index to query.</param>
        public string GetStringFromSequenceForIndex(int index)
        {
            string result = string.Empty;
            while (index >= 0 && this.CountSequence.Length > 0)
            {
                var wrappedIndex = index % this.CountSequence.Length;
                var value = this.CountSequence[wrappedIndex];
                result = string.Concat(value, result);
                index = (index / this.CountSequence.Length) - 1;

                if (this.DoNotCarryOver)
                {
                    break;
                }
            }

            return result;
        }
    }
}