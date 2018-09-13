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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;

    /// <summary>
    /// RenameOperationSequences are a collection of RenameOperations where the order is the order in which
    /// RenameOperations are applied to a string to get a resultant name.
    /// </summary>
    /// <typeparam name="T">The type of RenameOperation contained in the sequence</typeparam>
    public class RenameOperationSequence<T> : IList<T> where T : IRenameOperation
    {
        private List<T> operationSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameOperationSequence{T}"/> class.
        /// </summary>
        public RenameOperationSequence()
        {
            this.operationSequence = new List<T>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of items in the sequence.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return this.operationSequence.Count;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RenameOperation`1"/> at the specified index.
        /// </summary>
        /// <param name="index">Index to access.</param>
        /// <returns>The element at the specified index</returns>
        public T this[int index]
        {
            get
            {
                return this.operationSequence[index];
            }

            set
            {
                this.operationSequence[index] = value;
            }
        }

        /// <summary>
        /// Add an item to the end of the sequence.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item)
        {
            this.operationSequence.Add(item);
        }

        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <returns>The index of the specified item.</returns>
        /// <param name="item">Item to query.</param>
        public int IndexOf(T item)
        {
            return this.operationSequence.IndexOf(item);
        }

        /// <summary>
        /// Insert the specified item at the specified index.
        /// </summary>
        /// <param name="index">Index to insert the item into.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, T item)
        {
            this.operationSequence.Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.operationSequence.RemoveAt(index);
        }

        /// <summary>
        /// Clear all operations in the sequence.
        /// </summary>
        public void Clear()
        {
            this.operationSequence.Clear();
        }

        /// <summary>
        /// Returns true if the Sequence contains the specified item.
        /// </summary>
        /// <param name="item">Item to query for.</param>
        /// <returns>true if the item is found in the sequence, false otherwise.</returns>
        public bool Contains(T item)
        {
            return this.operationSequence.Contains(item);
        }

        /// <summary>
        /// Copies the sequence operations into an array.
        /// </summary>
        /// <param name="array">Array to copy into.</param>
        /// <param name="arrayIndex">Array index.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.operationSequence.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove the specified item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>true if the object is removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            return this.operationSequence.Remove(item);
        }

        /// <summary>
        /// Gets the enumerator for the IEnumerable.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.operationSequence.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the IEnumerable.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a preview of how the sequence would apply to a string with a given count.
        /// </summary>
        /// <returns>The rename preview.</returns>
        /// <param name="originalName">Original name.</param>
        /// <param name="count">Count, used for enumerating rename operations.</param>
        public RenameResultSequence GetRenamePreview(string originalName, int count)
        {
            var renameResults = this.GetRenameSequenceForName(originalName, count);
            var resultSequence = new RenameResultSequence(renameResults);

            return resultSequence;
        }

        /// <summary>
        /// Gets the resulting name from the sequence.
        /// </summary>
        /// <returns>The resulting name.</returns>
        /// <param name="originalName">Original name.</param>
        /// <param name="count">Count, used for enumerating rename operations.</param>
        public string GetResultingName(string originalName, int count)
        {
            var resultSequence = this.GetRenamePreview(originalName, count);
            return resultSequence.NewName;
        }

        /// <summary>
        /// Converts the sequence into a string that can be used to serialize it
        /// </summary>
        /// <returns>The serializable string.</returns>
        public string ToSerializableString()
        {
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var op in this.operationSequence)
            {
                if (!op.Equals(this.operationSequence[0]))
                {
                    stringBuilder.Append('\n');
                }

                stringBuilder.Append(ConvertOperationToStringEntry(op));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates an operation sequence from its serialized string
        /// </summary>
        /// <returns>The operation sequence.</returns>
        /// <param name="str">Formerly serialized string.</param>
        public static RenameOperationSequence<IRenameOperation> FromString(string str)
        {
            var sequence = new RenameOperationSequence<IRenameOperation>();
            var lines = str.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                sequence.Add(GetOperationFromStringEntry(line));
            }

            return sequence;
        }

        private static string ConvertOperationToStringEntry(IRenameOperation op)
        {
            return string.Format("[{0}]{1}", op.GetType(), JsonUtility.ToJson(op));
        }

        private static IRenameOperation GetOperationFromStringEntry(string entry)
        {
            // Capture the type inside brackets, and capture the rest of the line
            var typeRegex = new Regex("\\[([^\\]]*)\\](.*)\\n*");
            var typeMatch = typeRegex.Match(entry);
            var opTypeStr = typeMatch.Groups[1].Value;
            var opJson = typeMatch.Groups[2].Value;

            System.Type opType = System.Type.GetType(opTypeStr, true);

            return (IRenameOperation)JsonUtility.FromJson(opJson, opType);
        }

        private List<RenameResult> GetRenameSequenceForName(string originalName, int count)
        {
            var renameResults = new List<RenameResult>();
            string modifiedName = originalName;
            RenameResult result;

            if (this.operationSequence.Count == 0)
            {
                result = new RenameResult();
                result.Add(new Diff(originalName, DiffOperation.Equal));
                renameResults.Add(result);
            }
            else
            {
                foreach (var op in this.operationSequence)
                {
                    result = op.Rename(modifiedName, count);
                    renameResults.Add(result);
                    modifiedName = result.Result;
                }
            }

            return renameResults;
        }
    }
}