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
    using UnityEngine;

    /// <summary>
    /// Diff contains a single change to a string, represented as an insertion, deletion, or no change (equal)
    /// </summary>
    public class Diff
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Diff"/> class.
        /// </summary>
        /// <param name="result">Result of the diff.</param>
        /// <param name="operation">Operation the diff applied.</param>
        public Diff(string result, DiffOperation operation)
        {
            this.Result = result;
            this.Operation = operation;
        }

        /// <summary>
        /// Gets the resulting string of the Diff.
        /// </summary>
        /// <value>The result.</value>
        public string Result { get; private set; }

        /// <summary>
        /// Gets the operation the diff applied.
        /// </summary>
        /// <value>The operation applied.</value>
        public DiffOperation Operation { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="RedBlueGames.MulliganRenamer.Diff"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="RedBlueGames.MulliganRenamer.Diff"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="RedBlueGames.MulliganRenamer.Diff"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Diff otherDiff = obj as Diff;
            if ((object)otherDiff == null)
            {
                return false;
            }

            if (!this.Result.Equals(otherDiff.Result))
            {
                return false;
            }

            if (!this.Operation.Equals(otherDiff.Operation))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="RedBlueGames.MulliganRenamer.Diff"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="RedBlueGames.MulliganRenamer.Diff"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="RedBlueGames.MulliganRenamer.Diff"/>.</returns>
        public override string ToString()
        {
            return string.Format(
                "[Diff: Result={0}, Operation={1}]",
                this.Result,
                this.Operation);
        }
    }
}