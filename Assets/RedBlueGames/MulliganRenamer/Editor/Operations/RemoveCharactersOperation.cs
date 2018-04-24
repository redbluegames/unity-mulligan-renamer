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
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class RemoveCharactersOperation : IRenameOperation
    {
        public static readonly RemoveCharactersOperation.Configuration Symbols = new RemoveCharactersOperation.Configuration()
        {
            CharactersToRemove = "^\\s\\w",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };

        public static readonly RemoveCharactersOperation.Configuration Numbers = new RemoveCharactersOperation.Configuration()
        {
            CharactersToRemove = "\\d",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };

        private ReplaceStringOperation internalReplaceStringOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCharactersOperation"/> class.
        /// </summary>
        public RemoveCharactersOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCharactersOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public RemoveCharactersOperation(RemoveCharactersOperation operationToCopy)
        {
            this.CopyFrom(operationToCopy);
        }

        /// <summary>
        /// Gets or sets the options used to configure the Rename Operation.
        /// </summary>
        /// <value>The options.</value>
        public Configuration Config { get; set; }

        private ReplaceStringOperation InternalReplaceStringOperation
        {
            get
            {
                if (this.internalReplaceStringOperation == null)
                {
                    this.internalReplaceStringOperation = new ReplaceStringOperation();
                }

                this.internalReplaceStringOperation.SearchIsCaseSensitive = this.Config.IsCaseSensitive;
                this.internalReplaceStringOperation.UseRegex = true;

                var regexPattern = this.Config.CharactersToRemove;
                if (!this.Config.CharactersAreRegex)
                {
                    regexPattern = Regex.Escape(regexPattern);
                }

                var charactersAsRegex = string.Concat("[", regexPattern, "]");
                this.internalReplaceStringOperation.SearchString = charactersAsRegex;
                this.internalReplaceStringOperation.ReplacementString = string.Empty;

                return this.internalReplaceStringOperation;
            }
        }

        /// <summary>
        /// Checks if the operation has errors and returns true if it does.
        /// </summary>
        /// <returns><c>true</c>, if errors exist, <c>false</c> otherwise.</returns>
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
            var clone = new RemoveCharactersOperation(this);
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

            return this.InternalReplaceStringOperation.Rename(input, relativeCount);
        }

        /// <summary>
        /// Copies the state from one operation into this one
        /// </summary>
        /// <param name="other">Other.</param>
        public void CopyFrom(RemoveCharactersOperation other)
        {
            this.Config = other.Config;
        }

        /// <summary>
        /// Options used to configure RemoveCharactersOperations.
        /// </summary>
        public struct Configuration
        {
            /// <summary>
            /// Gets or sets the characters to remove.
            /// </summary>
            /// <value>The characters to remove.</value>
            public string CharactersToRemove { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this
            /// <see cref="RemoveCharactersOperation+RemoveCharactersOperationOptions"/>
            /// characters are regex symbols.
            /// </summary>
            /// <value><c>true</c> if characters are regex; otherwise, <c>false</c>.</value>
            public bool CharactersAreRegex { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the characters are matched using case sensitivity.
            /// </summary>
            /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
            public bool IsCaseSensitive { get; set; }
        }
    }
}