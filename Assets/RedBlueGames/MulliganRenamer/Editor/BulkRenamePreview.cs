namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class BulkRenamePreview
    {
        private List<RenamePreview> renamePreviews;

        private List<RenamePreview> RenamePreviews
        {
            get
            {
                if (this.renamePreviews == null)
                {
                    this.renamePreviews = new List<RenamePreview>();
                }

                return this.renamePreviews;
            }
        }

        public int NumObjects
        {
            get
            {
                return this.RenamePreviews.Count;
            }
        }

        public bool HasWarnings
        {
            get
            {
                for (int i = 0; i < this.NumObjects; ++i)
                {
                    if (this.HasWarningForIndex(i))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Add(UnityEngine.Object obj, RenameResultSequence renameResultSequence)
        {
            var renamePreview = new RenamePreview(obj, renameResultSequence);
            this.RenamePreviews.Add(renamePreview);
        }

        public RenameResultSequence GetRenameResultAtIndex(int index)
        {
            return this.RenamePreviews[index].RenameResultSequence;
        }

        public UnityEngine.Object GetOriginalObjectAtIndex(int index)
        {
            return this.RenamePreviews[index].ObjectToRename;
        }

        public void SetWarningForIndex(int index, bool warning)
        {
            this.RenamePreviews[index].HasWarnings = warning;
        }

        public bool HasWarningForIndex(int index)
        {
            return this.RenamePreviews[index].HasWarnings;
        }

        public bool ContainsObject(UnityEngine.Object obj)
        {
            return this.GetRenameResultForObject(obj) != null;
        }

        public RenameResultSequence GetRenameResultForObject(UnityEngine.Object obj)
        {
            foreach (var renamePreview in this.RenamePreviews)
            {
                if (renamePreview.ObjectToRename == obj)
                {
                    return renamePreview.RenameResultSequence;
                }
            }

            return null;
        }

        private class RenamePreview
        {
            public UnityEngine.Object ObjectToRename { get; private set; }

            public RenameResultSequence RenameResultSequence { get; private set; }

            public bool HasWarnings { get; set; }

            public RenamePreview(UnityEngine.Object obj, RenameResultSequence renameResultSequence)
            {
                this.ObjectToRename = obj;
                this.RenameResultSequence = renameResultSequence;
            }
        }
    }
}