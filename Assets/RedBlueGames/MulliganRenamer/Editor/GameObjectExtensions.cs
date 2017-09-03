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
    /// Extensions for the GameObject class
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the hierarchy sorting for the GameObject. This is like Sibling index only weighted based on the
        /// position of the object in the hierarchy. It should match the way it displays in the Unity scene.
        /// </summary>
        /// <returns>The hierarchy sorting of the GameObject.</returns>
        /// <param name="obj">GameObject to get the sorting for.</param>
        public static float GetHierarchySorting(this GameObject obj)
        {
            if (obj.transform.parent == null)
            {
                return obj.transform.GetSiblingIndex();
            }
            else
            {
                // Sorting =  parent.Sorting + siblingWeight * (siblingIndex + 1)
                // A = 0
                //  - a1 0 + (0.34 * 1) = 0.34
                //    - a2 0.34 + (0.17 * 1) = 0.51
                //  - b1 0 + (0.34 * 2) = 0.64
                // B = 1
                float weightFromSiblingIndex = obj.transform.GetSiblingWeight() * (obj.transform.GetSiblingIndex() + 1);
                return obj.transform.parent.gameObject.GetHierarchySorting() + weightFromSiblingIndex;
            }
        }

        private static float GetSiblingWeight(this Transform t)
        {
            if (t.parent == null)
            {
                return 1;
            }
            else
            {
                // SiblingWeight is the product of the weight of the parent divided amongst their children.
                // An element with 5 siblings would have a sibling weight of .2.
                return t.parent.GetSiblingWeight() / (t.parent.childCount + 1);
            }
        }
    }
}