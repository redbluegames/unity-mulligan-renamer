/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
            var validRenames = new List<ObjectNameDelta>();

            foreach (var renamedAsset in this.renamedAssets)
            {
                if (!DoesObjectStillExist(renamedAsset.NamedObject))
                {
                    continue;
                }

                validRenames.Add(renamedAsset);
            }

            BulkRenamer.ApplyNameDeltas(validRenames, true);
        }

        /// <summary>
        /// Reverts the name changes on the affected assets.
        /// </summary>
        public void RevertNameChange()
        {
            var reversedNames = new List<ObjectNameDelta>();
            foreach (var renamedAsset in this.renamedAssets)
            {
                if (!DoesObjectStillExist(renamedAsset.NamedObject))
                {
                    continue;
                }

                reversedNames.Add(new ObjectNameDelta(renamedAsset.NamedObject, renamedAsset.OldName));
            }

            BulkRenamer.ApplyNameDeltas(reversedNames, true);
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