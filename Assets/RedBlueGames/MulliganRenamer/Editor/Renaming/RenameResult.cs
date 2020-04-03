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
    using UnityEngine;

    /// <summary>
    /// <see cref="RenameResult"/> contains the diffs and helper functions to get string 
    /// results for string operations.
    /// </summary>
    public class RenameResult : ICollection<Diff>, IEnumerable<Diff>
    {
        public static readonly RenameResult Empty = new RenameResult();

        private List<Diff> diffs;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameResult"/> class.
        /// </summary>
        public RenameResult()
        {
            this.diffs = new List<Diff>();
        }

        /// <summary>
        /// Gets the count of Diffs.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return this.diffs.Count;
            }
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
        /// Gets the resultant name from the diffs
        /// </summary>
        /// <value>The new name.</value>
        public string Result
        {
            get
            {
                var name = string.Empty;
                foreach (var diff in this.diffs)
                {
                    if (diff.Operation == DiffOperation.Equal)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                    else if (diff.Operation == DiffOperation.Insertion)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                }

                return name;
            }
        }

        /// <summary>
        /// Gets the original value before diffs are applied.
        /// </summary>
        /// <value>The original.</value>
        public string Original
        {
            get
            {
                var name = string.Empty;
                foreach (var diff in this.diffs)
                {
                    if (diff.Operation == DiffOperation.Equal)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                    else if (diff.Operation == DiffOperation.Deletion)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                }

                return name;
            }
        }

        public Diff this[int key]
        {
            get
            {
                return this.diffs[key];
            }
            
            set
            {
                this.diffs[key] = value;
            }
        }

        /// <summary>
        /// Clear the RenameResult.
        /// </summary>
        public void Clear()
        {
            this.diffs.Clear();
        }

        /// <summary>
        /// Get whether or not the RenameResult contains the specified diff
        /// </summary>
        /// <param name="item">Item to check for.</param>
        /// <returns>True if the collection contains the diff, false otherwise</returns>
        public bool Contains(Diff item)
        {
            return this.diffs.Contains(item);
        }

        /// <summary>
        /// Copies the diffs from an array.
        /// </summary>
        /// <param name="array">Array to copy.</param>
        /// <param name="arrayIndex">Array index.</param>
        public void CopyTo(Diff[] array, int arrayIndex)
        {
            this.diffs.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove the specified item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was successfully removed from the collection, false otherwise</returns>
        public bool Remove(Diff item)
        {
            return this.diffs.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator for iterating the set.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<Diff> GetEnumerator()
        {
            return this.diffs.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Add the specified Diff to the collection.
        /// </summary>
        /// <param name="diffToAdd">Diff to add.</param>
        public void Add(Diff diffToAdd)
        {
            this.diffs.Add(diffToAdd);
        }

        /// <summary>
        /// Gets the original name with color tags added.
        /// </summary>
        /// <returns>The original name with color tags.</returns>
        /// <param name="deletionColor">Deletion color.</param>
        public string GetOriginalColored(Color32 deletionColor)
        {
            var startTag = string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(deletionColor), ">");
            var endTag = "</color>";
            return this.GetOriginalWithTags(startTag, endTag);
        }

        /// <summary>
        /// Gets the result of the diff collection with color tags added.
        /// </summary>
        /// <returns>The new name colored.</returns>
        /// <param name="insertionColor">Insertion color.</param>
        public string GetResultColored(Color32 insertionColor)
        {
            var startTag = string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(insertionColor), ">");
            var endTag = "</color>";
            return this.GetResultWithTags(startTag, endTag);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="RedBlueGames.MulliganRenamer.RenameResult"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="RedBlueGames.MulliganRenamer.RenameResult"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="RedBlueGames.MulliganRenamer.RenameResult"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherRenameResult = obj as RenameResult;
            if ((object)otherRenameResult == null)
            {
                return false;
            }

            if (this.diffs.Count != otherRenameResult.diffs.Count)
            {
                return false;
            }

            for (int i = 0; i < this.diffs.Count; ++i)
            {
                if (!this.diffs[i].Equals(otherRenameResult.diffs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="RedBlueGames.MulliganRenamer.RenameResult"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="RedBlueGames.MulliganRenamer.RenameResult"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="RedBlueGames.MulliganRenamer.RenameResult"/>.</returns>
        public override string ToString()
        {
            var str = string.Format("[RenameResult: NewName={0}, OriginalName={1}]", this.Result, this.Original);
            var diffs = string.Empty;
            foreach (var diff in this.diffs)
            {
                diffs = string.Concat(diffs, "\n", diff.ToString());
            }

            str = string.Concat(str, diffs);

            return str;
        }

        private string GetOriginalWithTags(string deletionTagStart, string deletionTagEnd)
        {
            var name = string.Empty;
            foreach (var diff in this.diffs)
            {
                if (diff.Operation == DiffOperation.Equal)
                {
                    name = string.Concat(name, diff.Result);
                }
                else if (diff.Operation == DiffOperation.Deletion)
                {
                    name = string.Concat(name, deletionTagStart, diff.Result, deletionTagEnd);
                }
            }

            return name;
        }

        private string GetResultWithTags(string insertionTagStart, string insertionTagEnd)
        {
            var name = string.Empty;
            foreach (var diff in this.diffs)
            {
                if (diff.Operation == DiffOperation.Equal)
                {
                    name = string.Concat(name, diff.Result);
                }
                else if (diff.Operation == DiffOperation.Insertion)
                {
                    name = string.Concat(name, insertionTagStart, diff.Result, insertionTagEnd);
                }
            }

            return name;
        }
    }
}