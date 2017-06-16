namespace RedBlueGames.BulkRename
{
    using System.Collections.Generic;
    using RedBlueGames.BulkRename.Dependencies.DiffMatchPatch;

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

            var differ = new diff_match_patch();
            this.Diffs = differ.diff_main(originalName, newName, false);
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

        private List<Diff> Diffs { get; set; }

        /// <summary>
        /// Gets the diff of the two names, formatted with color tags.
        /// </summary>
        /// <returns>The diff as a formatted string.</returns>
        /// <param name="colorTagForAdds">Color tag for adds.</param>
        /// <param name="colorTagForDeletes">Color tag for deletes.</param>
        public string GetDiffAsFormattedString(string colorTagForAdds, string colorTagForDeletes)
        {
            var diffedName = string.Empty;
            foreach (var diffResult in this.Diffs)
            {
                var segment = string.Empty;
                if (diffResult.operation == Operation.INSERT)
                {
                    segment = string.Concat(colorTagForAdds, diffResult.text, "</color>");
                }
                else if (diffResult.operation == Operation.DELETE)
                {
                    segment = string.Concat(colorTagForDeletes, diffResult.text, "</color>");
                }
                else
                {
                    segment = diffResult.text;
                }

                diffedName += segment;
            }

            return diffedName;
        }
    }
}