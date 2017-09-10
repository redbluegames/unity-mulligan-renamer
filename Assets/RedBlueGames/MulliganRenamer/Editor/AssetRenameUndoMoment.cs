namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Asset used to track the undo stack and Asset renames that should be renamed with it.
    /// </summary>
    public class AssetRenameUndoMoment : ScriptableObject
    {
        private List<ObjectNameDelta> renamedAssets;

        /// <summary>
        /// Gets or sets the last known name for the scriptableObject. This is used to track whether or not the asset changes
        /// were undone or redone.
        /// </summary>
        /// <value>The last known name of the scriptableObject.</value>
        public string LastKnownName { get; set; }

        /// <summary>
        /// Sets the renamed assets with their name history.
        /// </summary>
        /// <param name="objectRenames">Object renames.</param>
        public void SetRenamedAssets(List<ObjectNameDelta> objectRenames)
        {
            this.renamedAssets = objectRenames;
        }

        /// <summary>
        /// Reapplies the name changes to the affected assets.
        /// </summary>
        public void ReapplyNameChange()
        {
            var bulkRenamer = new BulkRenamer();
            var validRenames = new List<ObjectNameDelta>();

            foreach (var renamedAsset in this.renamedAssets)
            {
                if (!DoesObjectStillExist(renamedAsset.NamedObject))
                {
                    continue;
                }

                validRenames.Add(renamedAsset);
            }

            bulkRenamer.RenameObjects(validRenames, true);
        }

        /// <summary>
        /// Reverts the name changes on the affected assets.
        /// </summary>
        public void RevertNameChange()
        {
            var bulkRenamer = new BulkRenamer();
            var reversedNames = new List<ObjectNameDelta>();
            foreach (var renamedAsset in this.renamedAssets)
            {
                if (!DoesObjectStillExist(renamedAsset.NamedObject))
                {
                    continue;
                }

                reversedNames.Add(new ObjectNameDelta(renamedAsset.NamedObject, renamedAsset.OldName));
            }

            bulkRenamer.RenameObjects(reversedNames, true);
        }

        private static bool DoesObjectStillExist(UnityEngine.Object objectToTest)
        {
            var objectHasBeenDeleted = objectToTest == null || objectToTest.Equals(null);

            if (!objectHasBeenDeleted)
            {
                // Check if it's parent has been deleted
                if (AssetDatabase.IsSubAsset(objectToTest))
                {
                    var assetPath = AssetDatabase.GetAssetPath(objectToTest);
                    var parentAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    var parentIsDeleted = parentAsset == null || parentAsset.Equals(null);
                    return !parentIsDeleted;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}