namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Preview of all of the resultant renames from a BulkRename.
    /// </summary>
    public class BulkRenamePreview
    {
        private Dictionary<UnityEngine.Object, RenamePreview> renamePreviews;
        private List<RenamePreview> renamePreviewsList;

        /// <summary>
        /// Gets the number of objects in the preview.
        /// </summary>
        public int NumObjects
        {
            get
            {
                return this.renamePreviewsList.Count;
            }
        }

        /// <summary>
        /// Gets the number of steps in the rename sequence
        /// </summary>
        public int NumSteps
        {
            get
            {
                if (this.NumObjects == 0)
                {
                    return 0;
                }

                // All rename results sequences should have the same number of steps so just grab the first
                return this.renamePreviewsList[0].RenameResultSequence.NumSteps;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this preview contains objects with warnings.
        /// </summary>
        public bool HasWarnings
        {
            get
            {
                for (int i = 0; i < this.NumObjects; ++i)
                {
                    var preview = this.GetPreviewAtIndex(i);
                    if (preview.HasWarnings)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public BulkRenamePreview(RenamePreview[] previews)
        {
            this.renamePreviews = new Dictionary<UnityEngine.Object, RenamePreview>();
            this.renamePreviewsList = new List<RenamePreview>(previews.Length);

            for (int i = 0; i < previews.Length; ++i)
            {
                this.AddEntry(previews[i]);
            }
        }

        /// <summary>
        /// Gets the preview for the object at the supplied index.
        /// </summary>
        /// <returns>The preview at index.</returns>
        /// <param name="index">Index to query.</param>
        public RenamePreview GetPreviewAtIndex(int index)
        {
            return this.renamePreviewsList[index];
        }

        /// <summary>
        /// Determines if the BulkPreview contains a preview for the specified object.
        /// </summary>
        /// <returns><c>true</c>, if object is in the bulk rename, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object to query.</param>
        public bool ContainsPreviewForObject(UnityEngine.Object obj)
        {
            return this.renamePreviews.ContainsKey(obj);
        }

        /// <summary>
        /// Gets the preview for the specified object.
        /// </summary>
        /// <returns>The preview for object.</returns>
        /// <param name="obj">Object to query.</param>
        public RenamePreview GetPreviewForObject(UnityEngine.Object obj)
        {
            return this.renamePreviews[obj];
            }

        private void AddEntry(RenamePreview singlePreview)
        {
            // Keeping a list and a dictionary for fast access by index and by object...
            this.renamePreviews.Add(singlePreview.ObjectToRename, singlePreview);
            this.renamePreviewsList.Add(singlePreview);
        }
    }
}