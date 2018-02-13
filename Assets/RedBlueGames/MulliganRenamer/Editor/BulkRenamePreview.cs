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
                return this.RenamePreviewsList.Count;
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

        private Dictionary<UnityEngine.Object, RenamePreview> RenamePreviews
        {
            get
            {
                if (this.renamePreviews == null)
                {
                    this.renamePreviews = new Dictionary<UnityEngine.Object, RenamePreview>();
                }

                return this.renamePreviews;
            }
        }

        private List<RenamePreview> RenamePreviewsList
        {
            get
            {
                if (this.renamePreviewsList == null)
                {
                    this.renamePreviewsList = new List<RenamePreview>();
                }

                return this.renamePreviewsList;
            }
        }


        /// <summary>
        /// Adds a preview entry into the bulk preview
        /// </summary>
        /// <param name="singlePreview">Single preview.</param>
        public void AddEntry(RenamePreview singlePreview)
        {
            // Keeping a list and a dictionary for fast access by index and by object...
            // Not good to have to keep both structures in sync, though.
            this.RenamePreviews.Add(singlePreview.ObjectToRename, singlePreview);
            this.RenamePreviewsList.Add(singlePreview);
        }

        /// <summary>
        /// Gets the preview for the object at the supplied index.
        /// </summary>
        /// <returns>The preview at index.</returns>
        /// <param name="index">Index to query.</param>
        public RenamePreview GetPreviewAtIndex(int index)
        {
            return this.RenamePreviewsList[index];
        }

        /// <summary>
        /// Determines if the BulkPreview contains a preview for the specified object.
        /// </summary>
        /// <returns><c>true</c>, if object is in the bulk rename, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object to query.</param>
        public bool ContainsPreviewForObject(UnityEngine.Object obj)
        {
            return this.RenamePreviews.ContainsKey(obj);
        }

        /// <summary>
        /// Gets the preview for the specified object.
        /// </summary>
        /// <returns>The preview for object.</returns>
        /// <param name="obj">Object to query.</param>
        public RenamePreview GetPreviewForObject(UnityEngine.Object obj)
        {
            return this.RenamePreviews[obj];
        }
    }
}