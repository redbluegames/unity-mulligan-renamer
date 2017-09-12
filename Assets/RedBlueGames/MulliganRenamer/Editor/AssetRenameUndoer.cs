namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Rename undoer handles undoing renames of Assets, since they are not added to the Undo stack natively.
    /// </summary>
    [InitializeOnLoad]
    public static class AssetRenameUndoer
    {
        private static readonly string MomentNameBeforeRename = "Before";
        private static readonly string MomentNameAfterRename = "After";

        private static List<AssetRenameUndoMoment> undoMoments;

        static AssetRenameUndoer()
        {
            if (undoMoments == null)
            {
                undoMoments = new List<AssetRenameUndoMoment>();
            }

            Undo.undoRedoPerformed += OnUndoOrRedoPerformed;
        }

        /// <summary>
        /// Records the assets to rename, adding them to the Undo stack.
        /// </summary>
        /// <param name="undoName">Undo group name to associate the undo with.</param>
        /// <param name="objectsToRename">Objects to rename.</param>
        public static void RecordAssetRenames(string undoName, List<ObjectNameDelta> objectsToRename)
        {
            // Create a ScriptableObject which Unity's Undo system can track changes on.
            // Watch that ScriptableObject to determine if the user performs an Undo or a Redo.
            var moment = ScriptableObject.CreateInstance<AssetRenameUndoMoment>();
            undoMoments.Add(moment);

            moment.name = MomentNameBeforeRename;
            Undo.RecordObject(moment, undoName);

            var assets = objectsToRename.Where((obj) => obj.NamedObject.IsAsset()).ToList();
            moment.SetRenamedAssets(assets);

            moment.name = MomentNameAfterRename;
            moment.LastKnownName = moment.name;
        }

        private static void OnUndoOrRedoPerformed()
        {
            // Clear out moments that have gone out of memory... not sure how this happens, though.
            undoMoments = undoMoments.Where((moment) => moment != null).ToList();

            foreach (var moment in undoMoments)
            {
                if (!moment.name.Equals(moment.LastKnownName))
                {
                    if (moment.name.Equals(MomentNameAfterRename))
                    {
                        // Name change was redone
                        moment.ReapplyNameChange();
                    }
                    else
                    {
                        // Name change was undone
                        moment.RevertNameChange();
                    }

                    moment.LastKnownName = moment.name;
                }
            }
        }
    }
}