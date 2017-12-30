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
        private List<RenamePreview> renamePreviews;

        /// <summary>
        /// Gets the number of objects in the preview.
        /// </summary>
        public int NumObjects
        {
            get
            {
                return this.RenamePreviews.Count;
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

        /// <summary>
        /// Adds a preview entry into the bulk preview
        /// </summary>
        /// <param name="singlePreview">Single preview.</param>
        public void AddEntry(RenamePreview singlePreview)
        {
            this.RenamePreviews.Add(singlePreview);
        }

        /// <summary>
        /// Gets the preview for the object at the supplied index.
        /// </summary>
        /// <returns>The preview at index.</returns>
        /// <param name="index">Index to query.</param>
        public RenamePreview GetPreviewAtIndex(int index)
        {
            return this.RenamePreviews[index];
        }

        /// <summary>
        /// Determines if the BulkPreview contains a preview for the specified object.
        /// </summary>
        /// <returns><c>true</c>, if object is in the bulk rename, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object to query.</param>
        public bool ContainsPreviewForObject(UnityEngine.Object obj)
        {
            var previewForObject = this.GetPreviewForObject(obj);
            return previewForObject != null;
        }

        /// <summary>
        /// Gets the preview for the specified object.
        /// </summary>
        /// <returns>The preview for object.</returns>
        /// <param name="obj">Object to query.</param>
        public RenamePreview GetPreviewForObject(UnityEngine.Object obj)
        {
            foreach (var renamePreview in this.RenamePreviews)
            {
                if (renamePreview.ObjectToRename == obj)
                {
                    return renamePreview;
                }
            }

            return null;
        }
    }
}