namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Represents how the 'View' views an individual file and its selected-ness.
    /// </summary>
    internal class FileView
    {
        private FileInfo fileInfo;
        private bool isSelected;

        ////=====================================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="FileView"/> class from a System.IO.FileInfo
        /// </summary>
        /// <param name="fileInfo">System.IO.FileInfo to initialize this FileView with</param>
        internal FileView(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                throw new System.ArgumentException(string.Format("Unable to create File with path: {0}", fileInfo.FullName));
            }

            this.fileInfo = fileInfo;
        }

        private FileView()
        {
        }

        ////=====================================================================================================================

        /// <summary>
        /// Gets or sets a value indicating whether this FileView is selected/checked
        /// </summary>
        internal bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                this.isSelected = value;
            }
        }

        /// <summary>
        /// Gets this file's name (including extension)
        /// </summary>
        internal string FileName
        {
            get
            {
                return this.fileInfo.Name;
            }
        }

        /// <summary>
        /// Gets this file's full path and name (including extension) as a UniversalPath
        /// </summary>
        internal UniversalPath FullPathName
        {
            get
            {
                return (UniversalPath)this.fileInfo.FullName;
            }
        }

        /// <summary>
        /// Gets this FileView's GUIContent label for rendering in the GUI
        /// </summary>
        internal GUIContent Label
        {
            get
            {
                return new GUIContent(this.FileName, EditorGUIUtility.IconContent("cs Script Icon").image);
            }
        }

        ////=====================================================================================================================
    }
}