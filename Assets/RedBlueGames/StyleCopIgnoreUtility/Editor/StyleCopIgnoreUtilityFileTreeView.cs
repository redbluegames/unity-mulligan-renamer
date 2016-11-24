namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents an entire file tree with the most root directory.
    /// </summary>
    internal class FileTreeView
    {
        //// ====================================================================================================================

        private DirectoryView root;

        //// ====================================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTreeView"/> class., starting from the supplied path
        /// </summary>
        /// <param name="rootPath">A UniversalPath to the root of the FileTreeView to be created</param>
        internal FileTreeView(UniversalPath rootPath)
        {
            this.root = new DirectoryView(rootPath);
            this.root.IsExpanded = true;
        }

        private FileTreeView()
        {
        }

        //// ====================================================================================================================

        /// <summary>
        /// Gets the root of the FileTreeView
        /// </summary>
        internal DirectoryView Root
        {
            get
            {
                return this.root;
            }
        }

        //// ====================================================================================================================

        /// <summary>
        /// Gets a list of all of the selected/checked files in this FileTreeView
        /// </summary>
        /// <returns>Returns a list of UniversalPaths of all selected/checked files</returns>
        internal List<UniversalPath> GetSelectedFiles()
        {
            var allSelectedFiles = new List<UniversalPath>();
            this.root.AddSelectedFilesToList(ref allSelectedFiles);
            return allSelectedFiles;
        }

        /// <summary>
        /// Selects/Checks files contained in this FileTreeView from the supplied list
        /// </summary>
        /// <param name="filePaths">A list of UniversalPaths that contains all files to select/check</param>
        /// <returns>Returns true of the operation succeeds, otherwise false</returns>
        internal bool SetSelectedFiles(ref List<UniversalPath> filePaths)
        {
            return this.root.SetSelectedFilesFromList(ref filePaths);
        }
    }
}