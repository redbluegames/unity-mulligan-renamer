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
    /// RenameOperation that adds a string to the rename string.
    /// </summary>
    [System.Serializable]
    public class AddStringOperation : IRenameOperation
    {
        [SerializeField]
        private string prefix;

        [SerializeField]
        private string suffix;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddStringOperation"/> class.
        /// </summary>
        public AddStringOperation()
        {
            this.Prefix = string.Empty;
            this.Suffix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddStringOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public AddStringOperation(AddStringOperation operationToCopy)
        {
            this.Prefix = operationToCopy.Prefix;
            this.Suffix = operationToCopy.Suffix;
        }

        /// <summary>
        /// Gets or sets the prefix to add.
        /// </summary>
        public string Prefix
        {
            get
            {
                return this.prefix;
            }

            set
            {
                this.prefix = value;
            }
        }

        /// <summary>
        /// Gets or sets the suffix to add.
        /// </summary>
        public string Suffix
        {
            get
            {
                return this.suffix;
            }

            set
            {
                this.suffix = value;
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public IRenameOperation Clone()
        {
            var clone = new AddStringOperation(this);
            return clone;
        }

        public bool HasErrors()
        {
            return false;
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
            if (!string.IsNullOrEmpty(this.Prefix))
            {
                renameResult.Add(new Diff(this.Prefix, DiffOperation.Insertion));
            }

            if (!string.IsNullOrEmpty(input))
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            if (!string.IsNullOrEmpty(this.Suffix))
            {
                renameResult.Add(new Diff(this.Suffix, DiffOperation.Insertion));
            }

            return renameResult;
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
            hash = hash * 23 + this.Prefix.GetHashCode();
            hash = hash * 23 + this.Prefix.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as AddStringOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.Prefix != otherAsOp.Prefix)
            {
                return false;
            }

            if (this.Suffix != otherAsOp.Suffix)
            {
                return false;
            }

            return true;
        }
    }
}