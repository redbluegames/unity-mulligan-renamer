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
    /// Spritesheet renamer handles renaming all of the sprites that belong to a single spritesheet.
    /// This helps manage File IO and non-unique sprite names.
    /// </summary>
    public class SpritesheetRenamer
    {
        private Dictionary<Sprite, string> spritesAndNewNames;

        private Dictionary<Sprite, string> SpritesAndNewNames
        {
            get
            {
                if (this.spritesAndNewNames == null)
                {
                    this.spritesAndNewNames = new Dictionary<Sprite, string>();
                }

                return this.spritesAndNewNames;
            }
        }

        /// <summary>
        /// Adds a sprite for rename.
        /// </summary>
        /// <param name="sprite">Sprite to rename.</param>
        /// <param name="newName">New name for the sprite.</param>
        public void AddSpriteForRename(Sprite sprite, string newName)
        {
            var pathToSprite = AssetDatabase.GetAssetPath(sprite);

            var existingPathToTexture = this.GetPathToTextureForFirstSprite();
            if (!string.IsNullOrEmpty(existingPathToTexture) && pathToSprite != existingPathToTexture)
            {
                var exception = string.Format(
                                    LocalizationManager.Instance.GetTranslation("tryingToAddSpriteToRenamer"),
                                    sprite.name,
                                    pathToSprite,
                                    existingPathToTexture);
                throw new System.ArgumentException(exception);
            }

            var pathToMeta = pathToSprite + ".meta";
            if (!System.IO.File.Exists(pathToMeta))
            {
                var exception = string.Format(
                                    LocalizationManager.Instance.GetTranslation("errorTryingToAddSpriteToRenamer"),
                                    pathToMeta);
                throw new System.ArgumentException(exception);
            }

            // Unity doesn't let you name two sprites with the same name, so we shouldn't either.
            var uniqueName = this.CreateSpritesheetUniqueName(newName);

            this.SpritesAndNewNames.Add(sprite, uniqueName);
        }

        /// <summary>
        /// Renames the sprites.
        /// </summary>
        public void RenameSprites()
        {
            // Nothing to rename
            if (this.SpritesAndNewNames == null || this.spritesAndNewNames.Count == 0)
            {
                return;
            }

            // Get the path to the first sprite, so that we have an up to date
            // path to the texture (texture could get renamed in between when the 
            // sprites are added and later renamed).
            // We know all the sprites have the same path because it is enforced
            // in the Add function.
            var pathToTexture = GetPathToTextureForFirstSprite();
            var pathToTextureMetaFile = pathToTexture + ".meta";
            string metaFileWithRenames = System.IO.File.ReadAllText(pathToTextureMetaFile);

            var spritesAndUniqueNames = new Dictionary<Sprite, string>();
            foreach (var spriteNamePair in this.SpritesAndNewNames)
            {
                // First we set all sprites to a (almost) guaranteed unique name. This prevents us from
                // naming it the same as another sprite in the sheet, which can result in both sprites being renamed and
                // puts the spritesheet in an invalid state (no two sprites can share a name).
                var sprite = spriteNamePair.Key;
                var tempUniqueName = string.Concat(spriteNamePair.Value, Random.Range(100000, 999999));
                spritesAndUniqueNames.Add(sprite, tempUniqueName);
                metaFileWithRenames = ReplaceSpriteNameInMetaFile(metaFileWithRenames, sprite.name, tempUniqueName);
            }

            foreach (var uniqueSpriteNamePair in spritesAndUniqueNames)
            {
                var sprite = uniqueSpriteNamePair.Key;
                var uniqueName = uniqueSpriteNamePair.Value;

                // Strip off the random characters we added to make this sprite have a unique name
                var newName = uniqueName.Substring(0, uniqueName.Length - 6);
                metaFileWithRenames = ReplaceSpriteNameInMetaFile(metaFileWithRenames, uniqueName, newName);

                // Write the new name to the sprite so that the sprite in memory has the new name, or else undo
                // can't map it back to the previous sprite.
                sprite.name = newName;
            }

            // If users have hidden meta files they will get an access exception.
            // Need to unhide them briefly.
            var originalFileAttributes = System.IO.File.GetAttributes(pathToTextureMetaFile);
            System.IO.File.SetAttributes(pathToTextureMetaFile, originalFileAttributes & ~System.IO.FileAttributes.Hidden);
            System.IO.File.WriteAllText(pathToTextureMetaFile, metaFileWithRenames);
            System.IO.File.SetAttributes(pathToTextureMetaFile, originalFileAttributes);

            AssetDatabase.ImportAsset(pathToTexture);
        }

        private static string ReplaceSpriteNameInMetaFile(string metafileText, string spriteName, string newName)
        {
            string modifiedMetafile = ReplaceFileIDRecycleNames(metafileText, spriteName, newName);
            modifiedMetafile = ReplaceInternalIDToNameTable(modifiedMetafile, spriteName, newName);
            modifiedMetafile = ReplaceSpriteData(modifiedMetafile, spriteName, newName);
            return modifiedMetafile;
        }

        private static string ReplaceFileIDRecycleNames(string metafileText, string oldName, string newName)
        {
            string fileIDPattern = "([\\d]{8}: )" + System.Text.RegularExpressions.Regex.Escape(oldName) + "(\r?\n)";
            var fileIDRegex = new System.Text.RegularExpressions.Regex(fileIDPattern);
            string replacementText = "${1}" + newName + "${2}";
            return fileIDRegex.Replace(metafileText, replacementText);
        }

        private static string ReplaceSpriteData(string metafileText, string oldName, string newName)
        {
            string spritenamePattern = "(name: )" + System.Text.RegularExpressions.Regex.Escape(oldName) + "(\r?\n)";
            var spritenameRegex = new System.Text.RegularExpressions.Regex(spritenamePattern);
            string replacementText = "${1}" + newName + "${2}";
            return spritenameRegex.Replace(metafileText, replacementText);
        }

        private static string ReplaceInternalIDToNameTable(string metafileText, string oldName, string newName)
        {
            string spritenamePattern = "(second: )" + System.Text.RegularExpressions.Regex.Escape(oldName) + "(\r?\n)";
            var spritenameRegex = new System.Text.RegularExpressions.Regex(spritenamePattern);
            string replacementText = "${1}" + newName + "${2}";
            return spritenameRegex.Replace(metafileText, replacementText);
        }

        private string CreateSpritesheetUniqueName(string newName)
        {
            int repeats = 0;
            var uniqueName = newName;
            while (this.SpritesAndNewNames.ContainsValue(uniqueName))
            {
                repeats++;
                uniqueName = string.Concat(newName, "(" + repeats + ")");
            }

            return uniqueName;
        }

        private string GetPathToTextureForFirstSprite()
        {
            foreach (var spriteNamePair in this.SpritesAndNewNames)
            {
                return AssetDatabase.GetAssetPath(spriteNamePair.Key);
            }

            return string.Empty;
        }
    }
}