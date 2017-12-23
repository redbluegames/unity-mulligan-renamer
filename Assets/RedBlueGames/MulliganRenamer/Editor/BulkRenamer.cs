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
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Bulk renamer handles configuration and renaming of names.
    /// </summary>
    public class BulkRenamer
    {
        private RenameOperationSequence<RenameOperation> operationSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.MulliganRenamer.BulkRenamer"/> class.
        /// </summary>
        /// <param name="renameOperationSequence">Rename operation sequence to apply when renaming.</param>
        public BulkRenamer(RenameOperationSequence<RenameOperation> renameOperationSequence)
        {
            this.operationSequence = renameOperationSequence;
        }

        /// <summary>
        /// Renames the objects supplied as a list of object and new name pairings.
        /// </summary>
        /// <param name="objectsAndNewNames">Objects with their new names.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public static void ApplyNameDeltas(List<ObjectNameDelta> objectsAndNewNames, bool ignoreUndo = false)
        {
            List<ObjectNameDelta> assetsToRename;
            List<ObjectNameDelta> spritesToRename;
            List<ObjectNameDelta> gameObjectsToRename;
            SplitObjectsIntoCategories(objectsAndNewNames, out gameObjectsToRename, out assetsToRename, out spritesToRename);

            // Record all the objects to undo stack, note as of Unity 5.5.2 this does not record asset names,
            // so we have our own Undoer to handle assets.
            // Furthermore, our Spritesheet renaming isn't captured by the undo system so it must be manually undone.
            if (!ignoreUndo)
            {
                var gameObjectsToRenameAsGameObjects = new List<GameObject>();
                foreach (var gameObjectToRename in gameObjectsToRename)
                {
                    gameObjectsToRenameAsGameObjects.Add((GameObject)gameObjectToRename.NamedObject);
                }

                Undo.RecordObjects(gameObjectsToRenameAsGameObjects.ToArray(), "Bulk Rename");

                AssetRenameUndoer.RecordAssetRenames("Bulk Rename", objectsAndNewNames);
            }

            // Rename the objects and show a progress bar
            int totalNumSteps = spritesToRename.Count + assetsToRename.Count + gameObjectsToRename.Count;
            int progressBarStep = 0;
            var spritesheetRenamers = new List<SpritesheetRenamer>();
            var deferredRenames = new List<ObjectNameDelta>();
            foreach (var spriteToRename in spritesToRename)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                MarkSpriteForRename((Sprite)spriteToRename.NamedObject, spriteToRename.NewName, ref spritesheetRenamers);
            }

            foreach (var assetToRename in assetsToRename)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                var newName = string.Empty;
                if (RenamedAssetWillConflictWithAnotherAsset(assetToRename, objectsAndNewNames))
                {
                    // Decrement progress bar count because we'll increment it later when we do the deferred objects.
                    --progressBarStep;
                    deferredRenames.Add(assetToRename);
                    newName = assetToRename.NamedObject.GetInstanceID().ToString();
                }
                else
                {
                    newName = assetToRename.NewName;
                }

                RenameAsset(assetToRename.NamedObject, newName);
            }

            foreach (var gameObjectToRename in gameObjectsToRename)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                RenameGameObject((GameObject)gameObjectToRename.NamedObject, gameObjectToRename.NewName);
            }

            foreach (var deferredRename in deferredRenames)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                RenameAsset(deferredRename.NamedObject, deferredRename.NewName);
            }

            // Rename the sprites in the spritesheets
            for (int i = 0; i < spritesheetRenamers.Count; ++i, ++progressBarStep)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                spritesheetRenamers[i].RenameSprites();
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Renames the specified Objects according to a supplied RenameOperationSequence.
        /// </summary>
        /// <param name="objectsToRename">Objects to rename.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public void RenameObjects(List<UnityEngine.Object> objectsToRename, bool ignoreUndo = false)
        {
            var nameChanges = new List<ObjectNameDelta>();
            var previews = this.GetResultsPreview(objectsToRename);
            for (int i = 0; i < previews.NumObjects; ++i)
            {
                // Don't request a rename if the preview has warnings
                if (previews.HasWarningForIndex(i))
                {
                    continue;
                }

                var renamePreview = previews.GetPreviewAtIndex(i);
                var renameResult = renamePreview.RenameResultSequence;
                var newName = renameResult.NewName;
                var originalName = renameResult.OriginalName;

                // Don't request a rename if the name isn't going to change.
                if (originalName.Equals(newName))
                {
                    continue;
                }

                nameChanges.Add(new ObjectNameDelta(objectsToRename[i], newName));
            }

            BulkRenamer.ApplyNameDeltas(nameChanges, ignoreUndo);
        }

        /// <summary>
        /// Gets a preview of the Bulk Rename that shows the rename steps that will be applied
        /// to each object, if this renamer is used to rename the objects.
        /// </summary>
        /// <returns>The results preview.</returns>
        /// <param name="objectsToRename">Objects to rename.</param>
        public BulkRenamePreview GetResultsPreview(List<UnityEngine.Object> objectsToRename)
        {
            var renameResultPreviews = new BulkRenamePreview();
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                var singlePreview = new RenamePreview(
                                        objectsToRename[i], 
                                        this.operationSequence.GetRenamePreview(objectsToRename[i].name, i));
                renameResultPreviews.AddEntry(singlePreview);
            }

            var indecesWithErrors = GetIndecesOfPreviewsWithDuplicateNames(renameResultPreviews);
            foreach (var index in indecesWithErrors)
            {
                renameResultPreviews.SetWarningForIndex(index, true);
            }

            return renameResultPreviews;
        }

        private static List<int> GetIndecesOfPreviewsWithDuplicateNames(BulkRenamePreview preview)
        {
            // Iterate through all previews and:
            // Check to make sure the new name won't overlap with any existing files (in the directory)
            // - If the existing file is in the rename batch...
            //    - If the new name for the existing file will overlap with this one, warn.
            //    - If the new name for the existing file doesn't match this new one, we're ok. We fix it at runtime.
            // - If the existing file is not in this batch, we can't fix it during the rename. Show a warning.
            var problemIndeces = new List<int>();
            for (int i = 0; i < preview.NumObjects; ++i)
            {
                var previewForObject = preview.GetPreviewAtIndex(i);
                var thisObject = previewForObject.ObjectToRename;
                if (!AssetDatabase.Contains(thisObject))
                {
                    // Scene objects can be named the same thing, so skip these
                    continue;
                }

                // If this object isn't being renamed, don't check it for warnings.
                // This eliminates an issue where you'd always get two warnings -
                // one for the object being renamed and one for the object it collides with.
                var thisResult = previewForObject.RenameResultSequence;
                if (thisResult.NewName == thisResult.OriginalName)
                {
                    continue;
                }

                var assetPathToObject = AssetDatabase.GetAssetPath(thisObject);
                var assetsInDirectory = AssetDatabaseUtility.LoadAssetsAtDirectory(assetPathToObject);
                foreach (var assetInDirectory in assetsInDirectory)
                {
                    // Skip the current asset
                    if (assetInDirectory == thisObject)
                    {
                        continue;
                    }

                    // Objects can have the same name if they aren't the same type.
                    if (assetInDirectory.GetType() != thisObject.GetType())
                    {
                        continue;
                    }

                    string otherObjectName;
                    if (preview.ContainsPreviewForObject(assetInDirectory))
                    {
                        // Objects in the bulk rename only pose a problem if the new names will collide.
                        // If the names collide during the rename process, bulk renamer will fix them as it goes.
                        var assetPreview = preview.GetPreviewForObject(assetInDirectory);
                        RenameResultSequence otherObject = assetPreview.RenameResultSequence;
                        otherObjectName = otherObject.NewName;
                    }
                    else
                    {
                        otherObjectName = assetInDirectory.name;
                    }

                    if (otherObjectName == thisResult.NewName)
                    {
                        problemIndeces.Add(i);
                    }
                }
            }

            return problemIndeces;
        }

        private static bool RenamedAssetWillConflictWithAnotherAsset(
            ObjectNameDelta assetToRename,
            List<ObjectNameDelta> otherRenames)
        {
            var originalAssetPath = AssetDatabase.GetAssetPath(assetToRename.NamedObject);
            var futurePathToAsset = string.Concat(
                                        System.IO.Path.GetDirectoryName(originalAssetPath),
                                        System.IO.Path.DirectorySeparatorChar,
                                        assetToRename.NewName,
                                        System.IO.Path.GetExtension(originalAssetPath));

            foreach (var otherRename in otherRenames)
            {
                // Make sure to skip itself if it happens to be in the list
                var otherAsset = otherRename.NamedObject;
                if (otherAsset == assetToRename.NamedObject)
                {
                    continue;
                }

                var pathToOtherAsset = AssetDatabase.GetAssetPath(otherAsset);
                if (pathToOtherAsset == futurePathToAsset)
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateProgressBar(int currentStep, int totalNumSteps)
        {
            var infoString = string.Format("Renaming Object {0} of {1}", currentStep++, totalNumSteps);
            EditorUtility.DisplayProgressBar("Renaming...", infoString, currentStep / (float)totalNumSteps);
        }

        private static void SplitObjectsIntoCategories(
            List<ObjectNameDelta> objectRenames,
            out List<ObjectNameDelta> gameObjects,
            out List<ObjectNameDelta> assets,
            out List<ObjectNameDelta> sprites)
        {
            gameObjects = new List<ObjectNameDelta>();
            assets = new List<ObjectNameDelta>();
            sprites = new List<ObjectNameDelta>();
            foreach (var objectRename in objectRenames)
            {
                var obj = objectRename.NamedObject;
                if (obj.IsAsset())
                {
                    if (obj is Sprite)
                    {
                        sprites.Add(objectRename);
                    }
                    else
                    {
                        assets.Add(objectRename);
                    }
                }
                else
                {
                    gameObjects.Add(objectRename);
                }
            }
        }

        private static void MarkSpriteForRename(Sprite sprite, string newName, ref List<SpritesheetRenamer> spritesheetRenamers)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            SpritesheetRenamer existingSpritesheetRenamer = null;
            for (int i = 0; i < spritesheetRenamers.Count; ++i)
            {
                if (spritesheetRenamers[i].PathToTexture == path)
                {
                    existingSpritesheetRenamer = spritesheetRenamers[i];
                    break;
                }
            }

            if (existingSpritesheetRenamer != null)
            {
                existingSpritesheetRenamer.AddSpriteForRename(sprite, newName);
            }
            else
            {
                var spritesheetRenamer = new SpritesheetRenamer();
                spritesheetRenamer.AddSpriteForRename(sprite, newName);

                spritesheetRenamers.Add(spritesheetRenamer);
            }
        }

        private static void RenameGameObject(GameObject gameObject, string newName)
        {
            gameObject.name = newName;
        }

        private static void RenameAsset(UnityEngine.Object asset, string newName)
        {
            var pathToAsset = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.RenameAsset(pathToAsset, newName);

            if (asset.name != newName)
            {
                var message = string.Format(
                                  "Asset [{0}] not renamed when trying to RenameAsset in BulkRenamer. " +
                                  "It may have been canceled because the new name was already taken by" +
                                  " an object at the same path. The new name may also have contained " +
                                  "special characters.\n" +
                                  "OriginalPath: {1}, New Name: {1}",
                                  asset.name,
                                  pathToAsset,
                                  newName);
                throw new System.OperationCanceledException(message);
            }
        }
    }
}