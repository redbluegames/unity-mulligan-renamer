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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Extensions for Lists
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Removes all null entries in the list.
        /// </summary>
        /// <param name="list">List to modify.</param>
        public static void RemoveNullObjects<T>(this List<T> list)
        {
            list.RemoveAll(item => item == null || item.Equals(null));
        }

        public static void MoveElementFromIndexToIndex<T>(this List<T> list, int fromIndex, int desiredIndex)
        {
            desiredIndex = Mathf.Clamp(desiredIndex, 0, list.Count - 1);
            var destinationElementCopy = list[desiredIndex];
            list[desiredIndex] = list[fromIndex];
            list[fromIndex] = destinationElementCopy;
        }
    }
}