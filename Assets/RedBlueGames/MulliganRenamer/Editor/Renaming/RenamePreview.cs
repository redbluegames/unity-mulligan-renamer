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
    /// Preview for a single object's rename sequence.
    /// </summary>
    public class RenamePreview
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.MulliganRenamer.RenamePreview"/> class.
        /// </summary>
        /// <param name="objectToRename">Object to rename.</param>
        /// <param name="renameResultSequence">Rename result sequence.</param>
        public RenamePreview(UnityEngine.Object objectToRename, RenameResultSequence renameResultSequence)
        {
            this.ObjectToRename = objectToRename;
            this.RenameResultSequence = renameResultSequence;
            this.OriginalPathToObject = AssetDatabase.GetAssetPath(this.ObjectToRename);
            this.OriginalPathToSubAsset = AssetDatabaseUtility.GetAssetPathWithSubAsset(this.ObjectToRename);
        }

        /// <summary>
        /// Gets the object to rename.
        /// </summary>
        public UnityEngine.Object ObjectToRename { get; private set; }

        /// <summary>
        /// Gets the rename result sequence.
        /// </summary>
        public RenameResultSequence RenameResultSequence { get; private set; }

        /// <summary>
        /// Gets the original path to the object, before renaming
        /// </summary>
        public string OriginalPathToObject { get; private set; }

        /// <summary>
        /// Gets the original path to the object, with an appended suffix if it's a subasset.
        /// </summary>
        public string OriginalPathToSubAsset { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has warnings.
        /// </summary>
        public bool HasWarnings
        {
            get
            {
                return this.HasInvalidEmptyFinalName || this.FinalNameContainsInvalidCharacters;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:RedBlueGames.MulliganRenamer.RenamePreview"/> has an invalid
        /// empty final name (Scene GameObjects can have empty names).
        /// </summary>
        public bool HasInvalidEmptyFinalName
        {
            get
            {
                // Scene objects can be empty names... even though that's a terrible idea. But still, it's allowed.
                if (!this.ObjectToRename.IsAsset())
                {
                    return false;
                }

                return string.IsNullOrEmpty(this.RenameResultSequence.NewName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:RedBlueGames.MulliganRenamer.RenamePreview"/> final name
        /// contains invalid characters.
        /// </summary>
        public bool FinalNameContainsInvalidCharacters
        {
            get
            {
                if (!this.ObjectToRename.IsAsset())
                {
                    // Scene objects can have symbols
                    return false;
                }

                // Sprites are actually allowed to have all the characters that are considered invalid in Assets
                if(this.ObjectToRename is Sprite)
                {
                    return false;
                }

                var invalidCharacters = new char[] { '?', '/', '<', '>', '\\', '|', '*', ':', '"' };
                return this.RenameResultSequence.NewName.IndexOfAny(invalidCharacters) >= 0;
            }
        }

        /// <summary>
        /// Gets the resulting path, with sub assets appended to a filepath with a directory separator.
        /// </summary>
        /// <returns>The resulting path.</returns>
        public string GetResultingPath()
        {
            var resultingPath = string.Empty;
            string newFilename = this.RenameResultSequence.NewName;
            if (AssetDatabase.IsSubAsset(this.ObjectToRename))
            {
                resultingPath = string.Concat(this.OriginalPathToObject, "/", newFilename);
            }
            else
            {
                var pathWithoutFilename = System.IO.Path.GetDirectoryName(this.OriginalPathToObject);
                resultingPath = string.Concat(
                    pathWithoutFilename,
                    System.IO.Path.DirectorySeparatorChar,
                    this.RenameResultSequence.NewName,
                    System.IO.Path.GetExtension(this.OriginalPathToObject));
            }

            return resultingPath;
        }
    }
}