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
        /// <summary>
        /// Renames the specified Objects according to a supplied RenameOperationSequence.
        /// </summary>
        /// <param name="objectsToRename">Objects to rename.</param>
        /// <param name="sequence">Sequence to use to generate new names.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public void RenameObjects(List<UnityEngine.Object> objectsToRename, RenameOperationSequence<RenameOperation> sequence, bool ignoreUndo = false)
        {
            var objs = new List<ObjectNameDelta>();
            for (int i = 0; i < objectsToRename.Count; ++i)
            {
                var newName = sequence.GetResultingName(objectsToRename[i].name, i);
                var originalName = objectsToRename[i].name;

                // Don't request a rename if the name isn't going to change.
                if (originalName.Equals(newName))
                {
                    continue;
                }

                objs.Add(new ObjectNameDelta(objectsToRename[i], newName));
            }

            this.RenameObjects(objs, ignoreUndo);
        }

        /// <summary>
        /// Renames the objects supplied as a list of object and new name pairings.
        /// </summary>
        /// <param name="objectsAndNewNames">Objects with their new names.</param>
        /// <param name="ignoreUndo">If set to <c>true</c> ignore undo.</param>
        public void RenameObjects(List<ObjectNameDelta> objectsAndNewNames, bool ignoreUndo = false)
        {
            // Record all the objects to undo stack, note as of Unity 5.5.2 this does not record asset names,
            // so we have our own Undoer to handle assets.
            // Furthermore, our Spritesheet renaming isn't captured by the undo system so it must be manually undone.
            if (!ignoreUndo)
            {
                var gameObjectsToRename = new List<GameObject>();
                foreach (var objectAndName in objectsAndNewNames)
                {
                    if (!objectAndName.NamedObject.IsAsset())
                    {
                        gameObjectsToRename.Add((GameObject)objectAndName.NamedObject);
                    }
                }

                Undo.RecordObjects(gameObjectsToRename.ToArray(), "Bulk Rename");

                AssetRenameUndoer.RecordAssetRenames("Bulk Rename", objectsAndNewNames);
            }

            EditorUtility.DisplayProgressBar("Renaming Assets...", "Renaming Assets...", 0.0f);
            var spritesheetRenamers = new List<SpritesheetRenamer>();
            for (int i = 0; i < objectsAndNewNames.Count; ++i)
            {
                var infoString = string.Format("Renaming Asset {0} of {1}", i, objectsAndNewNames.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, i / (float)objectsAndNewNames.Count);

                var objectToRename = objectsAndNewNames[i].NamedObject;
                var newName = objectsAndNewNames[i].NewName;
                if (objectToRename.IsAsset())
                {
                    if (objectToRename is Sprite)
                    {
                        this.MarkSpriteForRename((Sprite)objectToRename, newName, ref spritesheetRenamers);
                    }
                    else
                    {
                        this.RenameAsset(objectToRename, newName);
                    }
                }
                else
                {
                    this.RenameGameObject((GameObject)objectToRename, newName);
                }
            }

            for (int i = 0; i < spritesheetRenamers.Count; ++i)
            {
                var infoString = string.Format("Renaming Spritesheet {0} of {1}", i, spritesheetRenamers.Count);
                EditorUtility.DisplayProgressBar("Renaming Assets...", infoString, i / (float)spritesheetRenamers.Count);
                
                spritesheetRenamers[i].RenameSprites();
            }

            EditorUtility.ClearProgressBar();
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