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
        private AssetCache assetCache;
        private RenameOperationSequence<IRenameOperation> operationSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.MulliganRenamer.BulkRenamer"/> class.
        /// </summary>
        public BulkRenamer()
        {
            this.Initialize();
        }

        ~BulkRenamer()
        {
            AssetPostprocessorEvents.AssetsReimported.RemoveListener(this.HandleAssetsReimported);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.MulliganRenamer.BulkRenamer"/> class.
        /// </summary>
        /// <param name="renameOperationSequence">Rename operation sequence to apply when renaming.</param>
        public BulkRenamer(RenameOperationSequence<IRenameOperation> renameOperationSequence)
        {
            this.Initialize();

            this.operationSequence = renameOperationSequence;
        }

        /// <summary>
        /// Renames the objects supplied as a list of object and new name pairings.
        /// </summary>
        /// <returns>The number of successfully renamed objects.</returns>
        /// <param name="objectsAndNewNames">Objects with their new names.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public static int ApplyNameDeltas(List<ObjectNameDelta> objectsAndNewNames, bool ignoreUndo = false)
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

                Undo.RecordObjects(gameObjectsToRenameAsGameObjects.ToArray(), LocalizationManager.Instance.GetTranslation("bulkRename"));

                AssetRenameUndoer.RecordAssetRenames(LocalizationManager.Instance.GetTranslation("bulkRename"), objectsAndNewNames);
            }

            // Rename the objects and show a progress bar
            int numFailedRenames = 0;
            int totalNumSteps = spritesToRename.Count + assetsToRename.Count + gameObjectsToRename.Count;
            int progressBarStep = 0;
            var deferredRenames = new List<ObjectNameDelta>();
            foreach (var assetToRename in assetsToRename)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                var newName = string.Empty;
                if (RenamedAssetWillShareNameWithAnotherAsset(assetToRename, objectsAndNewNames))
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
                try
                {
                    RenameAsset(deferredRename.NamedObject, deferredRename.NewName);
                }
                catch (System.OperationCanceledException)
                {
                    numFailedRenames++;
                }
            }

            // Package all sprites into renamers so we can do the file IO
            // all at once.
            var spritesheetRenamers = new Dictionary<string, SpritesheetRenamer>();
            foreach (var spriteToRename in spritesToRename)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                MarkSpriteForRename((Sprite)spriteToRename.NamedObject, spriteToRename.NewName, ref spritesheetRenamers);
            }

            // Rename the sprites in the spritesheets.
            foreach (var kvp in spritesheetRenamers)
            {
                UpdateProgressBar(progressBarStep++, totalNumSteps);
                kvp.Value.RenameSprites();
            }

            EditorUtility.ClearProgressBar();

            return totalNumSteps - numFailedRenames;
        }

        /// <summary>
        /// Sets the rename operations to use when renaming objects.
        /// </summary>
        /// <param name="renameOperationSequence">Rename operation sequence.</param>
        public void SetRenameOperations(RenameOperationSequence<IRenameOperation> renameOperationSequence)
        {
            this.operationSequence = renameOperationSequence;
        }

        /// <summary>
        /// Renames the specified Objects according to a supplied RenameOperationSequence.
        /// </summary>
        /// <returns>The number of successfully renamed objects.</returns>
        /// <param name="objectsToRename">Objects to rename.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public int RenameObjects(List<UnityEngine.Object> objectsToRename, bool ignoreUndo = false)
        {
            var nameChanges = new List<ObjectNameDelta>();
            var previews = this.GetBulkRenamePreview(objectsToRename);
            for (int i = 0; i < previews.NumObjects; ++i)
            {
                // Don't request a rename if the preview has warnings
                var renamePreview = previews.GetPreviewAtIndex(i);
                if (renamePreview.HasWarnings || previews.WillRenameCollideWithExistingAsset(renamePreview))
                {
                    continue;
                }

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

            return BulkRenamer.ApplyNameDeltas(nameChanges, ignoreUndo);
        }

        /// <summary>
        /// Gets a preview of the Bulk Rename that shows the rename steps that will be applied
        /// to each object, if this renamer is used to rename the objects.
        /// </summary>
        /// <returns>The results preview.</returns>
        /// <param name="objectsToRename">Objects to rename.</param>
        public BulkRenamePreview GetBulkRenamePreview(List<UnityEngine.Object> objectsToRename)
        {
            var previews = new RenamePreview[objectsToRename.Count];
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                var singlePreview = new RenamePreview(
                                        objectsToRename[i],
                                        this.operationSequence.GetRenamePreview(objectsToRename[i].name, i));
                previews[i] = singlePreview;
            }

            // First collect all assets in directories of preview objects into the assetCache.
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                var obj = objectsToRename[i];
                if (obj.IsAsset())
                {
                    CacheAsestsInSameDirectoryAsAsset(obj, ref this.assetCache);
                }
            }

            var renameResultPreviews = new BulkRenamePreview(previews, this.assetCache);

            return renameResultPreviews;
        }

        private static void CacheAsestsInSameDirectoryAsAsset(UnityEngine.Object asset, ref AssetCache assetCache)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var assetDirectory = AssetDatabaseUtility.GetDirectoryFromAssetPath(path);
            assetCache.LoadAssetsInAssetDirectory(assetDirectory);
        }

        private static bool RenamedAssetWillShareNameWithAnotherAsset(
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
            var infoString = string.Format(LocalizationManager.Instance.GetTranslation("renamingObjectXofY"), currentStep++, totalNumSteps);
            EditorUtility.DisplayProgressBar(LocalizationManager.Instance.GetTranslation("renaming")+ "...", infoString, currentStep / (float)totalNumSteps);
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

        private static void MarkSpriteForRename(Sprite sprite, string newName, ref Dictionary<string, SpritesheetRenamer> spritesheetRenamers)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            if (spritesheetRenamers.ContainsKey(path))
            {
                spritesheetRenamers[path].AddSpriteForRename(sprite, newName);
            }
            else
            {
                var spritesheetRenamer = new SpritesheetRenamer();
                spritesheetRenamer.AddSpriteForRename(sprite, newName);

                spritesheetRenamers[path] = spritesheetRenamer;
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
                                  LocalizationManager.Instance.GetTranslation("errorAssetNotBulkRenamed"),
                                  asset.name,
                                  pathToAsset,
                                  newName);
                throw new System.OperationCanceledException(message);
            }
        }

        private void Initialize()
        {
            this.assetCache = new AssetCache();
            AssetPostprocessorEvents.AssetsReimported.AddListener(this.HandleAssetsReimported);
        }

        private void HandleAssetsReimported()
        {
            this.assetCache.Clear();
        }
    }
}