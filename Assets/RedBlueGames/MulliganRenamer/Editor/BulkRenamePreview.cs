namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;

    /// <summary>
    /// Bulk rename preview contains the data for the results of a BulkRename operation.
    /// </summary>
    public class BulkRenamePreview
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.BulkRenamePreview"/> class.
        /// </summary>
        /// <param name="originalName">Original name.</param>
        /// <param name="newName">New name.</param>
        public BulkRenamePreview(string originalName, string newName)
        {
            this.OriginalName = originalName;
            this.NewName = newName;
        }

        /// <summary>
        /// Gets the original name, before any rename operation.
        /// </summary>
        /// <value>The original name.</value>
        public string OriginalName { get; private set; }

        /// <summary>
        /// Gets the new name, after the rename operations.
        /// </summary>
        /// <value>The new name.</value>
        public string NewName { get; private set; }
    }
}