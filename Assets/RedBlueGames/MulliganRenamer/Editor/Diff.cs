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