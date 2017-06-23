namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Represents how the 'View' views a directory (with recursive sub-directories), and manages directory selected-ness.
    /// </summary>
    internal class DirectoryView
    {
        private DirectoryInfo directoryInfo;
        private List<DirectoryView> subDirectories;
        private List<FileView> files;
        private bool isExpanded;

        ////=====================================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryView" /> class.
        /// </summary>
        /// <param name="path">UniversalPath to construct the DirectoryView from</param>
        internal DirectoryView(UniversalPath path)
        {
            if (!System.IO.Directory.Exists((string)path))
            {
                throw new System.ArgumentException(string.Format("Attempting to create a Directory with invalid path: {0}", path));
            }

            this.Build(new DirectoryInfo((string)path));
        }

        private DirectoryView()
        {
        }

        private DirectoryView(DirectoryInfo directoryInfo)
        {
            this.Build(directoryInfo);
        }

        ////=====================================================================================================================

        /// <summary>
        /// Gets the list of DirectoryViews that represent the sub-directories in this directory
        /// </summary>
        internal List<DirectoryView> SubDirectories
        {
            get
            {
                return this.subDirectories;
            }
        }

        /// <summary>
        /// Gets the list of FileViews that represent the files in this directory
        /// </summary>
        internal List<FileView> Files
        {
            get
            {
                return this.files;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this directory is expanded/collapsed in the GUI
        /// </summary>
        internal bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
            }
        }

        /// <summary>
        /// Gets this directory's name (without slashes)
        /// </summary>
        internal string DirectoryName
        {
            get
            {
                return this.directoryInfo.Name;
            }
        }

        /// <summary>
        /// Gets this directory's full path as a UniversalPath
        /// </summary>
        internal UniversalPath FullPathName
        {
            get
            {
                return (UniversalPath)this.directoryInfo.FullName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this DirectoryView is completely selected
        /// </summary>
        internal bool IsSelected
        {
            get
            {
                foreach (var subDir in this.subDirectories)
                {
                    if (!subDir.IsSelected)
                    {
                        return false;
                    }
                }

                return this.GetNumSelectedFiles() == this.files.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this DirectoryView is partially selected
        /// </summary>
        internal bool IsPartiallySelected
        {
            get
            {
                var numSelectedFiles = this.GetNumSelectedFiles();
                if ((numSelectedFiles > 0) && (numSelectedFiles < this.files.Count))
                {
                    return true;
                }

                var numSelectedSubDirectories = 0;
                foreach (var subDir in this.subDirectories)
                {
                    if (subDir.IsPartiallySelected)
                    {
                        return true;
                    }

                    if (subDir.IsSelected)
                    {
                        numSelectedSubDirectories++;
                    }
                }

                if (numSelectedSubDirectories > 0)
                {
                    if (numSelectedSubDirectories < this.subDirectories.Count)
                    {
                        return true;
                    }
                    else
                    {
                        return (this.files.Count > 0) && (numSelectedFiles < this.files.Count);
                    }
                }

                //// No sub-directories are selected

                return (this.subDirectories.Count > 0) && (numSelectedFiles > 0);
            }
        }

        /// <summary>
        /// Gets this DirectoryView's GUIContent label for rendering in the GUI
        /// </summary>
        internal GUIContent Label
        {
            get
            {
                return new GUIContent(this.DirectoryName, EditorGUIUtility.IconContent("Folder Icon").image);
            }
        }

        ////=====================================================================================================================

        /// <summary>
        /// Toggles whether to select all files and sub-directories contained within this directory
        /// </summary>
        internal void ToggleSelectAll()
        {
            this.SetSelectAll(!this.IsSelected);
        }

        /// <summary>
        /// Appends all currently-selected files to the supplied list
        /// </summary>
        /// <param name="selectedFiles">List of UniversalPaths to append selected files to</param>
        internal void AddSelectedFilesToList(ref List<UniversalPath> selectedFiles)
        {
            foreach (var subDirectory in this.subDirectories)
            {
                subDirectory.AddSelectedFilesToList(ref selectedFiles);
            }

            foreach (var file in this.files)
            {
                if (file.IsSelected)
                {
                    selectedFiles.Add(file.FullPathName);
                }
            }
        }

        /// <summary>
        /// Selects/Checks all files and sub-directories contained within this directory based on the supplied list
        /// </summary>
        /// <param name="filePathsToSelect">List of UniversalPaths to select</param>
        /// <returns>Returns true if the operation succeeded, otherwise false</returns>
        internal bool SetSelectedFilesFromList(ref List<UniversalPath> filePathsToSelect)
        {
            foreach (var file in this.files)
            {
                if (filePathsToSelect.Contains(file.FullPathName))
                {
                    file.IsSelected = true;
                    filePathsToSelect.Remove(file.FullPathName);
                }
                else
                {
                    file.IsSelected = false;
                }
            }

            foreach (var subDir in this.subDirectories)
            {
                subDir.SetSelectedFilesFromList(ref filePathsToSelect);
            }

            return true;
        }

        private void Build(DirectoryInfo newDirectoryInfo)
        {
            if (!newDirectoryInfo.Exists)
            {
                throw new System.ArgumentException(string.Format("Unable to create DirectoryInfo with path: {0}", newDirectoryInfo.FullName));
            }

            this.directoryInfo = newDirectoryInfo;
            this.subDirectories = new List<DirectoryView>();
            this.files = new List<FileView>();

            var fileInfos = this.directoryInfo.GetFiles("*.cs");
            foreach (var fileInfo in fileInfos)
            {
                this.files.Add(new FileView(fileInfo));
            }

            var subDirectoryInfos = this.directoryInfo.GetDirectories();
            foreach (var subDirectoryInfo in subDirectoryInfos)
            {
                this.subDirectories.Add(new DirectoryView(subDirectoryInfo));
            }

            // Remove empty sub directories
            for (int i = 0; i < this.subDirectories.Count; ++i)
            {
                if ((this.subDirectories[i].files.Count <= 0) && (this.subDirectories[i].subDirectories.Count <= 0))
                {
                    this.subDirectories.RemoveAt(i--);
                }
            }
        }

        private int GetNumSelectedFiles()
        {
            int numSelectedFiles = 0;
            for (int i = 0; i < this.files.Count; ++i)
            {
                if (this.files[i].IsSelected)
                {
                    numSelectedFiles++;
                }
            }

            return numSelectedFiles;
        }

        private void SetSelectAll(bool isSelected)
        {
            foreach (var file in this.files)
            {
                file.IsSelected = isSelected;
            }

            foreach (var subDirectory in this.subDirectories)
            {
                subDirectory.SetSelectAll(isSelected);
            }
        }
    }
}