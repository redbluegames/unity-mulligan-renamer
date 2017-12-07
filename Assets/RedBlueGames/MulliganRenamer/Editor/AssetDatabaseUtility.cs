namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

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