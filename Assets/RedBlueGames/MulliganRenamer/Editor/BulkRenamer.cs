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
                if (RenamedAssetWillConflictWithAnotherAsset(assetToRename, objectsAndNewNames))
                {
                    // Decrement progress bar count because we'll increment it later when we do the deferred objects.
                    --progressBarStep;
                    deferredRenames.Add(assetToRename);
                    var tempname = assetToRename.NamedObject.GetInstanceID().ToString();
                    RenameAsset(assetToRename.NamedObject, tempname);
                }
                else
                {
                    RenameAsset(assetToRename.NamedObject, assetToRename.NewName);
                }
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
        /// Gets a list of the RenameResultsSequences that show the rename steps that will be applied
        /// to each object, if this renamer is used to rename the objects.
        /// </summary>
        /// <returns>The results preview.</returns>
        /// <param name="objectsToRename">Objects to rename.</param>
        public List<RenameResultSequence> GetResultsPreview(List<UnityEngine.Object> objectsToRename)
        {
            var renameResultPreviews = new List<RenameResultSequence>();
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                renameResultPreviews.Add(
                    this.operationSequence.GetRenamePreview(objectsToRename[i].name, i));
            }

            return renameResultPreviews;
        }

        /// <summary>
        /// Renames the specified Objects according to a supplied RenameOperationSequence.
        /// </summary>
        /// <param name="objectsToRename">Objects to rename.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public void RenameObjects(List<UnityEngine.Object> objectsToRename, bool ignoreUndo = false)
        {
            var nameChanges = new List<ObjectNameDelta>();
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                var newName = this.operationSequence.GetResultingName(objectsToRename[i].name, i);
                var originalName = objectsToRename[i].name;

                // Don't request a rename if the name isn't going to change.
                if (originalName.Equals(newName))
                {
                    continue;
                }

                nameChanges.Add(new ObjectNameDelta(objectsToRename[i], newName));
            }

            BulkRenamer.ApplyNameDeltas(nameChanges, ignoreUndo);
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
        }
    }
}