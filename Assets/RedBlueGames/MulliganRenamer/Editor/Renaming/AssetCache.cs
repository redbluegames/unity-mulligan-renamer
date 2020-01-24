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
    using UnityEngine;

    /// <summary>
    /// Asset cache stores loaded assets so that we don't have to use too much
    /// file IO.
    /// </summary>
    public class AssetCache
    {
        private Dictionary<string, List<Object>> cachedAssetsInDirectories;
        private HashSet<string> cachedFilePaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.AssetCache"/> class.
        /// </summary>
        public AssetCache()
        {
            this.cachedAssetsInDirectories = new Dictionary<string, List<Object>>();
            this.cachedFilePaths = new HashSet<string>();
        }

        /// <summary>
        /// Loads the assets in the specified asset relative directory. Caches them for quick repeat access.
        /// </summary>
        /// <returns>The assets in the directory.</returns>
        /// <param name="assetRelativePath">Asset relative path to the directory.</param>
        public List<Object> LoadAssetsInAssetDirectory(string assetRelativePath)
        {
            // Load the assets in the directory or get the previously loaded ones.
            List<Object> assetsInDirectory;
            if (this.cachedAssetsInDirectories.ContainsKey(assetRelativePath))
            {
                assetsInDirectory = this.cachedAssetsInDirectories[assetRelativePath];
            }
            else
            {
                assetsInDirectory = AssetDatabaseUtility.LoadAssetsAtDirectory(assetRelativePath);
                this.AddAssets(assetRelativePath, assetsInDirectory);
            }

            return assetsInDirectory;
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            this.cachedAssetsInDirectories = new Dictionary<string, List<Object>>();
            this.cachedFilePaths = new HashSet<string>();
        }

        /// <summary>
        /// Gets all file paths hashed.
        /// </summary>
        /// <returns>The all file paths hashed.</returns>
        public HashSet<string> GetAllPathsHashed()
        {
            var copy = new HashSet<string>(this.cachedFilePaths);
            return copy;
        }

        private void AddAssets(string assetRelativePath, List<UnityEngine.Object> assets)
        {
            foreach (var asset in assets)
            {
                if (asset == null)
                {
                    continue;
                }

                var path = AssetDatabaseUtility.GetAssetPathWithSubAsset(asset);
                if (!string.IsNullOrEmpty(path))
                {
                    cachedFilePaths.Add(path);
                }
            }

            this.cachedAssetsInDirectories[assetRelativePath] = assets;
        }
    }
}