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
    /// Rename operation that adds a string from a sequence to the end of a target string.
    /// The sequence string is chosen based on the relative count passed into the Rename.
    /// </summary>
    public class AddStringSequenceOperation : IRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.AddStringSequenceOperation"/> class.
        /// </summary>
        public AddStringSequenceOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.AddStringSequenceOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public AddStringSequenceOperation(AddStringSequenceOperation operationToCopy)
        {
            this.StringSequence = new string[operationToCopy.StringSequence.Length];
            operationToCopy.StringSequence.CopyTo(this.StringSequence, 0);
        }

        /// <summary>
        /// Gets or sets the string sequence, which is iterated over based on the rename count.
        /// </summary>
        public string[] StringSequence { get; set; }

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
            var clone = new AddStringSequenceOperation(this);
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

            if (this.StringSequence != null && this.StringSequence.Length > 0)
            {
                var wrappedIndex = relativeCount % this.StringSequence.Length;
                var stringToInsert = this.StringSequence[wrappedIndex];
                if (!string.IsNullOrEmpty(stringToInsert))
                {
                    renameResult.Add(new Diff(this.StringSequence[wrappedIndex], DiffOperation.Insertion));
                }
            }

            return renameResult;
        }
    }
}