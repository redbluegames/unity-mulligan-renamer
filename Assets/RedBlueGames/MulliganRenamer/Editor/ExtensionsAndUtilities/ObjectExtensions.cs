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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Extension methods for UnityObject's.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determines if the specified Unity object is an Asset
        /// </summary>
        /// <returns><c>true</c> if the object is an asset; otherwise, <c>false</c>.</returns>
        /// <param name="obj">Object to test.</param>
        public static bool IsAsset(this UnityEngine.Object obj)
        {
            if (obj is GameObject)
            {
#if UNITY_2018_3_OR_NEWER
                return PrefabUtility.IsPartOfPrefabAsset(obj);
#else
                var prefabType = PrefabUtility.GetPrefabType(obj);
                return prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab;
#endif
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the editor icon for the Unity Object.
        /// </summary>
        /// <returns>The editor icon.</returns>
        /// <param name="unityObject">Unity object.</param>
        public static Texture GetEditorIcon(this UnityEngine.Object unityObject)
        {
            var pathToObject = AssetDatabase.GetAssetPath(unityObject);
            Texture icon = null;
            if (string.IsNullOrEmpty(pathToObject))
            {
                if (unityObject is GameObject)
                {
                    icon = EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image;
                }
                else
                {
                    icon = EditorGUIUtility.FindTexture("DefaultAsset Icon");
                }
            }
            else
            {
                if (unityObject is Sprite)
                {
                    icon = AssetPreview.GetAssetPreview(unityObject);
                }
                else
                {
                    icon = AssetDatabase.GetCachedIcon(pathToObject);
                }
            }

            return icon;
        }
    }
}