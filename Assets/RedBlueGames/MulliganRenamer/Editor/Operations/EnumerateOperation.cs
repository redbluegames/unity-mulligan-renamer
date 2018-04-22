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
    /// RenameOperation that enumerates characters onto the end of the rename string.
    /// </summary>
    public class EnumerateOperation : IRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerateOperation"/> class.
        /// </summary>
        public EnumerateOperation()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerateOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public EnumerateOperation(EnumerateOperation operationToCopy)
        {
            this.StartingCount = operationToCopy.StartingCount;
            this.CountFormat = operationToCopy.CountFormat;
            this.Increment = operationToCopy.Increment;

            this.Initialize();
        }

        /// <summary>
        /// Gets or sets the starting count.
        /// </summary>
        /// <value>The starting count.</value>
        public int StartingCount { get; set; }

        /// <summary>
        /// Gets or sets the format for the count, appended to the end of the string.
        /// </summary>
        /// <value>The count format.</value>
        public string CountFormat { get; set; }

        /// <summary>
        /// Gets or sets the increment to use when counting.
        /// </summary>
        public int Increment { get; set; }

        /// <summary>
        /// Gets a value indicating whether the count string format specified is parsable.
        /// </summary>
        public bool IsCountStringFormatValid
        {
            get
            {
                try
                {
                    this.StartingCount.ToString(this.CountFormat);
                    return true;
                }
                catch (System.FormatException)
                {
                    return false;
                }
            }
        }

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
            var clone = new EnumerateOperation(this);
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

            if (!string.IsNullOrEmpty(this.CountFormat))
            {
                var currentCount = this.StartingCount + (relativeCount * this.Increment);
                try
                {
                    var currentCountAsString = currentCount.ToString(this.CountFormat);
                    renameResult.Add(new Diff(currentCountAsString, DiffOperation.Insertion));
                }
                catch (System.FormatException)
                {
                    // Can't append anything if format is bad.
                }
            }

            return renameResult;
        }

        private void Initialize()
        {
            this.Increment = 1;

            // Give it an initially valid count format
            this.CountFormat = "0";
        }
    }
}