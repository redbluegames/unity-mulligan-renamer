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
        private List<IRenameOperation> renameOperations;

        private List<IRenameOperation> RenameOperations
        {
            get
            {
                if (this.renameOperations == null)
                {
                    this.renameOperations = new List<IRenameOperation>();
                }

                return this.renameOperations;
            }
        }

        /// <summary>
        /// Sets the rename operation to use.
        /// </summary>
        /// <param name="operation">Operation to assign.</param>
        public void SetRenameOperation(IRenameOperation operation)
        {
            var operationList = new List<IRenameOperation>();
            operationList.Add(operation);
            this.SetRenameOperations(operationList);
        }

        /// <summary>
        /// Sets the rename operations.
        /// </summary>
        /// <param name="operations">Operations to assign.</param>
        public void SetRenameOperations(List<IRenameOperation> operations)
        {
            this.RenameOperations.Clear();
            this.RenameOperations.AddRange(operations);
        }

        /// <summary>
        /// Gets previews for the rename operations to be performed on the supplied strings.
        /// </summary>
        /// <returns>The RenamePreviews.</returns>
        /// <param name="originalNames">Original names to rename.</param>
        public List<RenameResultSequence> GetRenamePreviews(params string[] originalNames)
        {
            var previews = new List<RenameResultSequence>(originalNames.Length);

            for (int i = 0; i < originalNames.Length; ++i)
            {
                var originalName = originalNames[i];
                var renameResults = this.GetRenameSequenceForName(originalName, i);
                previews.Add(new RenameResultSequence(renameResults));
            }

            return previews;
        }

        /// <summary>
        /// Renames the specified objects according to the rules of the BulkRenamer.
        /// </summary>
        /// <param name="objectsToRename">Objects to rename.</param>
        public void RenameObjects(List<UnityEngine.Object> objectsToRename)
        {
            // Record all the objects to undo stack, though this unfortunately doesn't capture Asset renames
            Undo.RecordObjects(objectsToRename.ToArray(), "Bulk Rename");

            var spritesAndNames = new Dictionary<Sprite, string>();
            var nonSpriteAssetsAndNames = new Dictionary<UnityEngine.Object, string>();
            var gameObjectsAndNames = new Dictionary<GameObject, string>();
            this.SeparateObjectsByRenameType(objectsToRename, ref spritesAndNames, ref nonSpriteAssetsAndNames, ref gameObjectsAndNames);

            EditorUtility.DisplayProgressBar("Renaming Assets...", "Collecting Sprites for Rename", 0.0f);
            var spritesheetRenamers = new List<SpritesheetRenamer>();
            foreach (var spriteNamePair in spritesAndNames)
            {
                this.MarkSpriteForRename(spriteNamePair.Key, spriteNamePair.Value, ref spritesheetRenamers);
            }

            int totalProgressSteps = spritesheetRenamers.Count + nonSpriteAssetsAndNames.Count + gameObjectsAndNames.Count;
            int currentProgressStep = 0;
            for (int i = 0; i < spritesheetRenamers.Count; ++i)
            {
                var infoString = string.Format("Renaming Spritesheet {0} of {1}", i, spritesheetRenamers.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                spritesheetRenamers[i].RenameSprites();

                currentProgressStep++;
            }

            int assetCount = 0;
            foreach (var assetNamePair in nonSpriteAssetsAndNames)
            {
                var infoString = string.Format("Renaming Asset {0} of {1}", assetCount, nonSpriteAssetsAndNames.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                this.RenameAsset(assetNamePair.Key, assetNamePair.Value);

                currentProgressStep++;
                assetCount++;
            }

            int gameObjectCount = 0;
            foreach (var gameObjectNamePair in gameObjectsAndNames)
            {
                var infoString = string.Format("Renaming GameObjects {0} of {1}", gameObjectCount, gameObjectsAndNames.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, currentProgressStep / (float)totalProgressSteps);
                this.RenameGameObject(gameObjectNamePair.Key, gameObjectNamePair.Value);

                currentProgressStep++;
                gameObjectCount++;
            }

            EditorUtility.ClearProgressBar();
        }

        private void SeparateObjectsByRenameType(
            List<UnityEngine.Object> objects,
            ref Dictionary<Sprite, string> sprites,
            ref Dictionary<UnityEngine.Object, string> nonSpriteAssets,
            ref Dictionary<GameObject, string> gameObjects)
        {
            var newNames = this.GetRenamePreviews(objects.GetNames());
            for (int i = 0; i < newNames.Count; ++i)
            {
                var objectToRename = objects[i];
                var newName = newNames[i].NewName;
                if (objectToRename.IsAsset())
                {
                    if (objectToRename is Sprite)
                    {
                        sprites[(Sprite)objectToRename] = newName;
                    }
                    else
                    {
                        nonSpriteAssets[objectToRename] = newName;
                    }
                }
                else
                {
                    gameObjects[(GameObject)objectToRename] = newName;
                }
            }
        }

        private List<RenameResult> GetRenameSequenceForName(string originalName, int count)
        {
            var renameResults = new List<RenameResult>();
            string modifiedName = originalName;
            RenameResult result;

            if (this.RenameOperations.Count == 0)
            {
                result = new RenameResult();
                result.Add(new Diff(originalName, DiffOperation.Equal));
                renameResults.Add(result);
            }
            else
            {
                foreach (var op in this.RenameOperations)
                {
                    result = op.Rename(modifiedName, count);
                    renameResults.Add(result);
                    modifiedName = result.Result;
                }
            }

            return renameResults;
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
    }
}