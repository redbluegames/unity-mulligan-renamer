namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Stores a Unity-relative path to a file or folder
    /// </summary>
    [System.Serializable]
    internal struct UniversalPath
    {
        //// Fields =============================================================================================================

        [SerializeField]
        private string relativePath;

        //// Constructors =======================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPath"/> struct from a full absolute path
        /// </summary>
        /// <param name="fullPath">An absolute path to a file or folder</param>
        public UniversalPath(string fullPath)
        {
            this.relativePath = ConvertToUnityRelativePath(fullPath);
        }

        //// Properties =========================================================================================================

        /// <summary>
        /// Gets a value indicating whether this <see cref="UniversalPath"/> is a valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                var fullPath = AddNonUnityPath(this.relativePath);

                if (string.IsNullOrEmpty(fullPath))
                {
                    return false;
                }

                if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets this <see cref="UniversalPath"/> as a Unity relative path
        /// </summary>
        public string RelativePath
        {
            get
            {
                return this.relativePath;
            }
        }

        //// Methods ============================================================================================================

        /// <summary>
        /// Converts the relative Unity path to an absolute path when explicitly cast to string
        /// </summary>
        /// <param name="universalPath">The UniversalPath to cast to string</param>
        public static explicit operator string(UniversalPath universalPath)
        {
            return AddNonUnityPath(universalPath.relativePath);
        }

        /// <summary>
        /// Converts the absolute path string to a relative Unity UniversalPath
        /// </summary>
        /// <param name="fullPath">An absolute path to a file or folder</param>
        public static explicit operator UniversalPath(string fullPath)
        {
            return new UniversalPath(fullPath);
        }

        /// <summary>
        /// Gets the string of the absolute path represented by this UniversalPath
        /// </summary>
        /// <returns>A string containing an absolute path</returns>
        public override string ToString()
        {
            return ((string)this).ToString();
        }

        private static string ConvertToUnityRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var nonUnityPath = new DirectoryInfo(Application.dataPath).Parent.FullName;

            path = path.Replace(@"\", "/");
            nonUnityPath = nonUnityPath.Replace(@"\", "/");

            return path.Replace(nonUnityPath, null);
        }

        private static string AddNonUnityPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var nonUnityPath = new DirectoryInfo(Application.dataPath).Parent.FullName;

            path = path.Replace(@"\", "/");
            nonUnityPath = nonUnityPath.Replace(@"\", "/");

            if (!path.Contains(nonUnityPath))
            {
                path = nonUnityPath + path;
            }

            return path;
        }
    }
}