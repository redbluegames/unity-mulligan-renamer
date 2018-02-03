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

        /// <summary>
        /// Gets the path to the associated texture with the sprites.
        /// </summary>
        /// <value>The path to texture.</value>
        public string PathToTexture { get; private set; }

        private string PathToTextureMetaFile
        {
            get
            {
                return this.PathToTexture + ".meta";
            }
        }

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
            if (!string.IsNullOrEmpty(this.PathToTexture) && pathToSprite != this.PathToTexture)
            {
                var exception = string.Format(
                                    "Trying to add Sprite {0} to SpriteRenamer that has a different path to texture " +
                                    "than the other sprites. Received path {1}, expected {2}",
                                    sprite.name,
                                    pathToSprite,
                                    this.PathToTexture);
                throw new System.ArgumentException(exception);
            }

            var pathToMeta = pathToSprite + ".meta";
            if (!System.IO.File.Exists(pathToMeta))
            {
                var exception = string.Format(
                                    "Trying to add Sprite to SpriteRenamer at path {0}, but " +
                                    "no meta file exists at the specified path.",
                                    pathToMeta);
                throw new System.ArgumentException(exception);
            }

            this.PathToTexture = pathToSprite;

            // Unity doesn't let you name two sprites with the same name, so we shouldn't either.
            var uniqueName = this.CreateSpritesheetUniqueName(newName);

            this.SpritesAndNewNames.Add(sprite, uniqueName);
        }

        /// <summary>
        /// Renames the sprites.
        /// </summary>
        public void RenameSprites()
        {
            string metaFileWithRenames = System.IO.File.ReadAllText(this.PathToTextureMetaFile);
            foreach (var spriteNamePair in this.SpritesAndNewNames)
            {
                var sprite = spriteNamePair.Key;
                metaFileWithRenames = ReplaceSpriteInMetaFile(metaFileWithRenames, sprite, spriteNamePair.Value);
                sprite.name = spriteNamePair.Value;
            }

            System.IO.File.WriteAllText(this.PathToTextureMetaFile, metaFileWithRenames);

            AssetDatabase.ImportAsset(this.PathToTexture);
        }

        private static string ReplaceSpriteInMetaFile(string metafileText, Sprite sprite, string newName)
        {
            string modifiedMetafile = ReplaceFileIDRecycleNames(metafileText, sprite.name, newName);
            modifiedMetafile = ReplaceSpriteData(modifiedMetafile, sprite.name, newName);
            return modifiedMetafile;
        }

        private static string ReplaceFileIDRecycleNames(string metafileText, string oldName, string newName)
        {
            string fileIDPattern = "([\\d]{8}: )" + oldName + "\n";
            var fileIDRegex = new System.Text.RegularExpressions.Regex(fileIDPattern);
            string replacementText = "$1" + newName + "\n";
            return fileIDRegex.Replace(metafileText, replacementText);
        }

        private static string ReplaceSpriteData(string metafileText, string oldName, string newName)
        {
            string spritenamePattern = "(name: )" + oldName + "\n";
            var spritenameRegex = new System.Text.RegularExpressions.Regex(spritenamePattern);
            string replacementText = "$1" + newName + "\n";
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
    }
}