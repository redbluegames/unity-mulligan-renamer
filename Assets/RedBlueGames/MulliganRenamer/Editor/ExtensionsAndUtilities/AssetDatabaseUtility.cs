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
        /// Gets the asset's path as a directory (excludes filename)
        /// </summary>
        /// <returns>The asset path directory.</returns>
        /// <param name="asset">Asset to query.</param>
        public static string GetAssetPathDirectory(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return GetDirectoryFromAssetPath(path);
        }

        /// <summary>
        /// Gets the directory for the specified asset path.
        /// </summary>
        /// <returns>The directory from the asset path.</returns>
        /// <param name="assetPath">Asset path.</param>
        public static string GetDirectoryFromAssetPath(string assetPath)
        {
            var directory = string.Concat(System.IO.Path.GetDirectoryName(assetPath), System.IO.Path.DirectorySeparatorChar);
            return directory;
        }

        /// <summary>
        /// Gets the asset path, with the name of subassets appended, if this is a subasset.
        /// </summary>
        /// <returns>The asset path including sub asset.</returns>
        /// <param name="asset">Asset or SubAsset.</param>
        public static string GetAssetPathWithSubAsset(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);

            if (AssetDatabase.IsSubAsset(asset))
            {
                path = string.Concat(path, "/", asset.name);
            }

            return path;
        }

        /// <summary>
        /// Loads all assets at the specified directory.
        /// </summary>
        /// <returns>The assets at the specified asset relative directory.</returns>
        /// <param name="assetDirectory">Asset relative directory.</param>
        public static List<UnityEngine.Object> LoadAssetsAtDirectory(string assetDirectory)
        {
            // Note this has to use AssetDatabase.Load so that we load sprites and other sub assets and
            // include them as "assets" in the directory. System.IO would not find these files.
            var assetGUIDsInDirectory = AssetDatabase.FindAssets(string.Empty, new string[] { assetDirectory.Substring(0, assetDirectory.Length - 1) });
            var filePaths = new List<string>(assetGUIDsInDirectory.Length);
            for (int i = 0; i < assetGUIDsInDirectory.Length; ++i)
            {
                var guid = assetGUIDsInDirectory[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                // Only need to add one of each path, because FindAssets returns the same guid multiple times
                // for sub assets (WTH Unity??)
                if (!filePaths.Contains(path))
                {
                    filePaths.Add(path);
                }
            }

            var assetsAtPath = new List<UnityEngine.Object>();
            foreach (var filePath in filePaths)
            {
                // Workaround: Scene assets for some reason error if you load them via LoadAllAssets.
                // (does it maybe try to load the contents inside the scene?)
                if (System.IO.Path.GetExtension(filePath) == ".unity")
                {
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
                    assetsAtPath.Add(sceneAsset);
                }
                else
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(filePath);
                    foreach (var subAsset in subAssets)
                    {
                        // It's possible for user created assets to include nulls...
                        // not sure how, but our AnimTemplates have a Null in them, so others
                        // may as well. Skip them.
                        if (subAsset != null)
                        {
                            // Ignore objects that are hidden in the hierarchy, as
                            // from the user's perspective they aren't there.
                            if ((subAsset.hideFlags & HideFlags.HideInHierarchy) != 0)
                            {
                                continue;
                            }

                            assetsAtPath.Add(subAsset);
                        }
                    }
                }
            }

            return assetsAtPath;
        }
    }
}