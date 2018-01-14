namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Preview for a single object's rename sequence.
    /// </summary>
    public class RenamePreview
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.MulliganRenamer.RenamePreview"/> class.
        /// </summary>
        /// <param name="objectToRename">Object to rename.</param>
        /// <param name="renameResultSequence">Rename result sequence.</param>
        public RenamePreview(UnityEngine.Object objectToRename, RenameResultSequence renameResultSequence)
        {
            this.ObjectToRename = objectToRename;
            this.RenameResultSequence = renameResultSequence;
        }

        /// <summary>
        /// Gets the object to rename.
        /// </summary>
        public UnityEngine.Object ObjectToRename { get; private set; }

        /// <summary>
        /// Gets the rename result sequence.
        /// </summary>
        public RenameResultSequence RenameResultSequence { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has warnings.
        /// </summary>
        public bool HasWarnings
        {
            get
            {
                return !string.IsNullOrEmpty(this.WarningMessage);
            }
        }

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets the resulting path, with sub assets appended to a filepath with a directory separator.
        /// </summary>
        /// <returns>The resulting path.</returns>
        public string GetResultingPath()
        {
            var pathToObject = AssetDatabase.GetAssetPath(this.ObjectToRename);

            var resultingPath = string.Empty;
            string newFilename = this.RenameResultSequence.NewName;
            if (AssetDatabase.IsSubAsset(this.ObjectToRename))
            {
                resultingPath = string.Concat(pathToObject, "/", newFilename);
            }
            else
            {
                var pathWithoutFilename = System.IO.Path.GetDirectoryName(pathToObject);
                resultingPath = string.Concat(
                    pathWithoutFilename,
                    this.RenameResultSequence.NewName,
                    System.IO.Path.GetExtension(pathToObject));
            }

            return resultingPath;
        }
    }
}