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
        private RenameOperationSequence<RenameOperation> RenameOperationSequence { get; set; }

        public void SetOperationSequence(RenameOperationSequence<RenameOperation> sequence)
        {
            this.RenameOperationSequence = sequence;
        }

        /// <summary>
        /// Renames the specified objects according to the rules of the BulkRenamer.
        /// </summary>
        /// <param name="objectsToRename">Objects to rename.</param>
        public void RenameObjects(List<UnityEngine.Object> objectsToRename)
        {
            // Record all the objects to undo stack, note as of Unity 5.5.2 this does not record asset names
            Undo.RecordObjects(objectsToRename.ToArray(), "Bulk Rename");
            //RenameUndoer.RecordAssets(join spritesAndNames, nonSpriteAssetsAndNames

            var spritesAndCounts = new List<ObjectCountPair<Sprite>>();
            var nonSpriteAssetsAndCounts = new List<ObjectCountPair<UnityEngine.Object>>();
            var gameObjectsAndCounts = new List<ObjectCountPair<GameObject>>();
            this.SeparateObjectsByRenameType(objectsToRename, ref spritesAndCounts, ref nonSpriteAssetsAndCounts, ref gameObjectsAndCounts);

            EditorUtility.DisplayProgressBar("Renaming Assets...", "Collecting Sprites for Rename", 0.0f);
            var spritesheetRenamers = new List<SpritesheetRenamer>();
            foreach (var spriteCountPair in spritesAndCounts)
            {
                var newName = this.RenameOperationSequence.GetResultingName(spriteCountPair.UnityObject.name, spriteCountPair.Count);
                this.MarkSpriteForRename((Sprite)spriteCountPair.UnityObject, newName, ref spritesheetRenamers);
            }

            int totalProgressSteps = spritesheetRenamers.Count + nonSpriteAssetsAndCounts.Count + gameObjectsAndCounts.Count;
            int currentProgressStep = 0;
            for (int i = 0; i < spritesheetRenamers.Count; ++i)
            {
                var infoString = string.Format("Renaming Spritesheet {0} of {1}", i, spritesheetRenamers.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                spritesheetRenamers[i].RenameSprites();

                currentProgressStep++;
            }

            int assetCount = 0;
            foreach (var assetNamePair in nonSpriteAssetsAndCounts)
            {
                var infoString = string.Format("Renaming Asset {0} of {1}", assetCount, nonSpriteAssetsAndCounts.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                var newName = this.RenameOperationSequence.GetResultingName(assetNamePair.UnityObject.name, assetNamePair.Count);
                this.RenameAsset(assetNamePair.UnityObject, newName);

                currentProgressStep++;
                assetCount++;
            }

            int gameObjectCount = 0;
            foreach (var gameObjectNamePair in gameObjectsAndCounts)
            {
                var infoString = string.Format("Renaming GameObjects {0} of {1}", gameObjectCount, gameObjectsAndCounts.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                var newName = this.RenameOperationSequence.GetResultingName(gameObjectNamePair.UnityObject.name, gameObjectNamePair.Count);
                this.RenameGameObject(gameObjectNamePair.UnityObject, newName);

                currentProgressStep++;
                gameObjectCount++;
            }

            EditorUtility.ClearProgressBar();
        }

        private void SeparateObjectsByRenameType(
            List<UnityEngine.Object> objects,
            ref List<ObjectCountPair<Sprite>> sprites,
            ref List<ObjectCountPair<UnityEngine.Object>> nonSpriteAssets,
            ref List<ObjectCountPair<GameObject>> gameObjects)
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                var objectToRename = objects[i];
                if (objectToRename.IsAsset())
                {
                    if (objectToRename is Sprite)
                    {
                        sprites.Add(new ObjectCountPair<Sprite>((Sprite)objectToRename, i));
                    }
                    else
                    {
                        nonSpriteAssets.Add(new ObjectCountPair<UnityEngine.Object>(objectToRename, i));
                    }
                }
                else
                {
                    gameObjects.Add(new ObjectCountPair<GameObject>((GameObject)objectToRename, i));
                }
            }
        }

        private void MarkSpriteForRename(Sprite sprite, string newName, ref List<SpritesheetRenamer> spritesheetRenamers)
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

        private void RenameGameObject(GameObject gameObject, string newName)
        {
            gameObject.name = newName;
        }

        private void RenameAsset(UnityEngine.Object asset, string newName)
        {
            var pathToAsset = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.RenameAsset(pathToAsset, newName);
        }

        private class ObjectCountPair<T> where T : UnityEngine.Object
        {
            public T UnityObject { get; }

            public int Count;

            public ObjectCountPair(T obj, int count)
            {
                this.UnityObject = obj;
                this.Count = count;
            }
        }
    }
}