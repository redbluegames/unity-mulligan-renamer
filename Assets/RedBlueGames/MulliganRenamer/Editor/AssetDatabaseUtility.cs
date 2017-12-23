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
    /// Utility (extensions) for the static AssetDatabase class
    /// </summary>
    public static class AssetDatabaseUtility
    {
        /// <summary>
        /// Loads all assets at the specified directory.
        /// </summary>
        /// <returns>The assets at the specified asset relative directory.</returns>
        /// <param name="assetDirectory">Asset relative directory.</param>
        public static List<UnityEngine.Object> LoadAssetsAtDirectory(string assetDirectory)
        {
            var directory = string.Concat(System.IO.Path.GetDirectoryName(assetDirectory), System.IO.Path.DirectorySeparatorChar);
            var pathToProject = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            var path = string.Concat(pathToProject, directory);
            string[] filePaths = System.IO.Directory.GetFiles(path);

            var assetsAtPath = new List<UnityEngine.Object>();
            foreach (var filePath in filePaths)
            {
                var extension = System.IO.Path.GetExtension(filePath);

                // Meta files track the asset, they aren't themselves the asset. Throw them out.
                if (extension == ".meta")
                {
                    continue;
                }

                // Textures have sprites in there. Add all assets in this file, including the file itself.
                var assetRelativePath = filePath.Substring(filePath.IndexOf("Assets/"));

                // Workaround: Scene assets for some reason freak out if you load them
                // (does it maybe try to load the contents inside the scene?)
                if (System.IO.Path.GetExtension(assetRelativePath) == ".unity")
                {
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetRelativePath);
                    assetsAtPath.Add(sceneAsset);
                }
                else
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetRelativePath);
                    assetsAtPath.AddRange(subAssets);
                }
            }

            return assetsAtPath;
        }
    }
}