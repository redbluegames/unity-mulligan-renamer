namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Diff
    {
        public string Result { get; private set; }

        public DiffOperation Operation { get; private set; }

        public Diff(string result, DiffOperation operation)
        {
            this.Result = result;
            this.Operation = operation;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Diff otherDiff = obj as Diff;
            if ((System.Object)otherDiff == null)
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(
                "[Diff: Result={0}, Operation={1}]",
                this.Result,
                this.Operation);
        }
    }
}