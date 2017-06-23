namespace RedBlueGames.StyleCopIgnoreUtility
{
    using UnityEngine;

    /// <summary>
    /// The data asset for the utility.
    /// </summary>
    [System.Serializable]
    internal class StyleCopIgnoreUtilityData : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private UniversalPath styleCopSettingsFilePath;

        [SerializeField]
        [HideInInspector]
        private UniversalPath[] ignoredFilePaths = new UniversalPath[0];

        /// <summary>
        /// Gets or sets the location of the saved Settings data (.asset) file for the Utility
        /// </summary>
        internal UniversalPath StyleCopSettingsFilePath
        {
            get
            {
                return this.styleCopSettingsFilePath;
            }

            set
            {
                this.styleCopSettingsFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of files to be saved in the Settings data (.asset) file
        /// </summary>
        internal UniversalPath[] IgnoredFilePaths
        {
            get
            {
                return this.ignoredFilePaths;
            }

            set
            {
                this.ignoredFilePaths = value;
            }
        }

        /// <summary>
        /// Determines whether the supplied list of file names is equal to the saved list (order matters)
        /// </summary>
        /// <param name="list">String array containing a list of filenames plus extensions (not full paths)</param>
        /// <returns>Returns true if the list of filenames matches, otherwise false</returns>
        internal bool IsIgnoredFilesEqualToFileNameList(string[] list)
        {
            if ((this.IgnoredFilePaths == null) || (list == null) || (this.IgnoredFilePaths.Length != list.Length))
            {
                return false;
            }

            for (int i = 0; i < this.IgnoredFilePaths.Length; ++i)
            {
                if (list[i] != new System.IO.FileInfo((string)this.IgnoredFilePaths[i]).Name)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
