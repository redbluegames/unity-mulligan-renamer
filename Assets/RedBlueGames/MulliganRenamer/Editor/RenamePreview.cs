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
        /// Gets or sets a value indicating whether this instance has warnings.
        /// </summary>
        public bool HasWarnings { get; set; }
    }
}