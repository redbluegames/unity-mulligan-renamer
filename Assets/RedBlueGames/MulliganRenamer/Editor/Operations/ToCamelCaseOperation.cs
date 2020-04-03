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
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation that changes the case of the words in the name
    /// </summary>
    [System.Serializable]
    public class ToCamelCaseOperation : IRenameOperation
    {
        [SerializeField]
        private bool usePascal;

        [SerializeField]
        private string delimiterCharacters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToCamelCaseOperation"/> class.
        /// </summary>
        public ToCamelCaseOperation()
        {
            this.delimiterCharacters = "- _";
            this.usePascal = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToCamelCaseOperation"/> class by copying another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ToCamelCaseOperation(ToCamelCaseOperation operationToCopy)
        {
            this.delimiterCharacters = operationToCopy.delimiterCharacters;
            this.usePascal = operationToCopy.usePascal;
        }

        public bool UsePascal
        {
            get
            {
                return this.usePascal;
            }

            set
            {
                this.usePascal = value;
            }
        }

        public bool UseCamel
        {
            get
            {
                return !this.UsePascal;
            }
        }

        public string DelimiterCharacters
        {
            get
            {
                return this.delimiterCharacters;
            }

            set
            {
                this.delimiterCharacters = value;
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
            var clone = new ToCamelCaseOperation(this);
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

            var inputCaseChanged = string.Empty;
            if (!string.IsNullOrEmpty(this.DelimiterCharacters))
            {
                // We use regex instead of string.Split simply because the results can include the delimiters using capture groups
                var delimiterCharArray = this.DelimiterCharacters.ToCharArray();
                var patternBuilder = new System.Text.StringBuilder();
                patternBuilder.Append("([");
                foreach (var delimiterChar in delimiterCharArray)
                {
                    // Dashes don't get escaped but are treated as a special character... need to escape them manually
                    if (delimiterChar == '-')
                    {
                        patternBuilder.Append("\\-");
                    }
                    else
                    {
                        patternBuilder.Append(Regex.Escape(delimiterChar.ToString()));
                    }
                }
                patternBuilder.Append("])");
                string pattern = patternBuilder.ToString();

                string[] words = Regex.Split(input, pattern);
                var changedStringBuilder = new System.Text.StringBuilder();
                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        // Do not capitalize the delimiter characters
                        if (word.Length == 1 && this.DelimiterCharacters.Contains(word))
                        {
                            changedStringBuilder.Append(word);
                        }
                        else
                        {
                            var isFirstWord = word == words[0];
                            var useLowerCasing = isFirstWord && this.UseCamel;
                            changedStringBuilder.Append(this.UpperOrLowerFirstChar(word, useLowerCasing));
                        }
                    }
                }

                inputCaseChanged = changedStringBuilder.ToString();
            }
            else
            {
                inputCaseChanged = input;
            }

            var renameResult = RenameResultUtilities.GetDiffResultFromStrings(input, inputCaseChanged);
            return renameResult;
        }

        /// <summary>
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            return this.delimiterCharacters.GetHashCode();
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as ToCamelCaseOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (this.DelimiterCharacters != otherAsOp.DelimiterCharacters)
            {
                return false;
            }

            return true;
        }

        private string UpperOrLowerFirstChar(string word, bool toLower)
        {
            if (string.IsNullOrEmpty(word))
            {
                return string.Empty;
            }

            var firstChar = word.Substring(0, 1);
            var wordCaseChanged = firstChar = toLower ? firstChar.ToLower() : firstChar.ToUpper();

            if (word.Length > 1)
            {
                wordCaseChanged = string.Concat(wordCaseChanged, word.Substring(1, word.Length - 1));
            }

            return wordCaseChanged;
        }
    }
}