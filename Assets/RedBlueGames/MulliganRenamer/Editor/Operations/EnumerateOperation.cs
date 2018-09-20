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
    [System.Serializable]
    public class EnumerateOperation : IRenameOperation
    {
        [SerializeField]
        private int startingCount;

        [SerializeField]
        private string countFormat;

        [SerializeField]
        private int increment;

        [SerializeField]
        private bool prepend;

        [SerializeField]
        private CountFormatPreset formatPreset;

        public enum CountFormatPreset
        {
            Custom = 0,
            SingleDigit = 1,
            LeadingZero = 2,
            Underscore = 3,
        }

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
            this.Initialize();

            this.formatPreset = operationToCopy.formatPreset;
            this.countFormat = operationToCopy.countFormat;

            this.StartingCount = operationToCopy.StartingCount;
            this.Increment = operationToCopy.Increment;
            this.Prepend = operationToCopy.Prepend;
        }

        /// <summary>
        /// Gets or sets the starting count.
        /// </summary>
        /// <value>The starting count.</value>
        public int StartingCount
        {
            get
            {
                return this.startingCount;
            }

            set
            {
                this.startingCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the format for the count, appended to the end of the string.
        /// </summary>
        /// <value>The count format.</value>
        public string CountFormat
        {
            get
            {
                if (this.formatPreset == CountFormatPreset.SingleDigit)
                {
                    return "0";
                }
                else if (this.formatPreset == CountFormatPreset.LeadingZero)
                {
                    return "00";
                }
                else if (this.formatPreset == CountFormatPreset.Underscore)
                {
                    return "_00";
                }
                else
                {
                    return this.countFormat;
                }
            }
        }

        /// <summary>
        /// Gets or sets the increment to use when counting.
        /// </summary>
        public int Increment
        {
            get
            {
                return this.increment;
            }

            set
            {
                this.increment = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to add the count to the front of the string
        /// </summary>
        public bool Prepend
        {
            get
            {
                return this.prepend;
            }

            set
            {
                this.prepend = value;
            }
        }

        public CountFormatPreset FormatPreset
        {
            get
            {
                return this.formatPreset;
            }
        }

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
            if (!string.IsNullOrEmpty(input) && !this.Prepend)
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

            if (this.Prepend)
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            return renameResult;
        }

        /// <summary>
        /// Sets a custom string format to use when counting.
        /// </summary>
        /// <param name="format">String format to use when counting</param>
        public void SetCountFormat(string format)
        {
            this.countFormat = format;
            this.formatPreset = CountFormatPreset.Custom;
        }


        /// <summary>
        /// Sets a format preset to use when counting.
        /// </summary>
        /// <param name="preset">Preset format to use when counting.</param>
        public void SetCountFormatPreset(CountFormatPreset preset)
        {
            this.formatPreset = preset;
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
            hash = hash * 23 + this.StartingCount.GetHashCode();
            hash = hash * 23 + this.Increment.GetHashCode();
            hash = hash * 23 + this.CountFormat.GetHashCode();
            hash = hash * 23 + this.FormatPreset.GetHashCode();
            hash = hash * 23 + this.Prepend.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as EnumerateOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.StartingCount != otherAsOp.StartingCount)
            {
                return false;
            }

            if (this.Increment != otherAsOp.Increment)
            {
                return false;
            }

            if (this.CountFormat != otherAsOp.CountFormat)
            {
                return false;
            }

            if (this.FormatPreset != otherAsOp.FormatPreset)
            {
                return false;
            }

            if (this.Prepend != otherAsOp.Prepend)
            {
                return false;
            }

            return true;
        }

        private void Initialize()
        {
            this.Increment = 1;

            // Give it an initially valid count format
            this.countFormat = "0";

            // Start in Single digit just because it's more readable and shows the user
            // what to do.
            this.formatPreset = CountFormatPreset.SingleDigit;
        }
    }
}