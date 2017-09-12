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