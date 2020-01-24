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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// UniqueList enforces all elements in the list are unique, allowing for constant time Contains
    /// via a hashset.
    /// </summary>
    public class UniqueList<T> : IList<T>
    {
        private List<T> objects;
        private HashSet<T> cachedObjects;

        public UniqueList()
        {
            this.objects = new List<T>();
            this.cachedObjects = new HashSet<T>();
        }

        public T this[int index]
        {
            get
            {
                return this.objects[index];
            }
            set
            {
                if (this.cachedObjects.Contains(value))
                {
                    var exceptionMessage = string.Format(
                        "Tried to add a repeat of an item to UniqueList. Item: {0}", value);
                    throw new InvalidOperationException(exceptionMessage);
                }

                // if we are replacing an element in the list, we need to replace it in the cache as well
                var existingObject = this.objects[index];
                if (typeof(T).IsValueType || existingObject != null)
                {
                    this.cachedObjects.Remove(existingObject);
                }

                this.objects[index] = value;
                this.cachedObjects.Add(value);
            }
        }

        public int Count
        {
            get
            {
                return this.objects.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            if (this.cachedObjects.Contains(item))
            {
                var exceptionMessage = string.Format(
                    "Tried to add a repeat of an item to UniqueList. Item: {0}", item);
                throw new System.InvalidOperationException(exceptionMessage);
            }

            this.objects.Add(item);
            this.cachedObjects.Add(item);
        }

        public void Clear()
        {
            this.objects.Clear();
            this.cachedObjects.Clear();
        }

        public bool Contains(T item)
        {
            return this.cachedObjects.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.objects.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.objects.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (this.cachedObjects.Contains(item))
            {
                var exceptionMessage = string.Format(
                    "Tried to insert a repeat of an item to UniqueList. Item: {0}", item);
                throw new System.InvalidOperationException(exceptionMessage);
            }

            this.objects.Insert(index, item);
            this.cachedObjects.Add(item);
        }

        public bool Remove(T item)
        {
            this.cachedObjects.Remove(item);
            return this.objects.Remove(item);
        }

        public void RemoveAt(int index)
        {
            var objectToRemove = this.objects[index];
            this.cachedObjects.Remove(objectToRemove);

            this.objects.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.objects.GetEnumerator();
        }

        public void RemoveNullObjects()
        {
            // There are never nulls if T is a value type.
            if (typeof(T).IsValueType)
            {
                return;
            }

            this.objects.RemoveNullObjects();
            this.cachedObjects.RemoveWhere(item => item == null || item.Equals(null));
        }

        public List<T> ToList()
        {
            // Return a copy so that users can't manipulate the order or contents of the internal list
            var copy = new List<T>();
            copy.AddRange(this.objects);
            return copy;
        }

        public void AddRange(List<T> collection)
        {
            foreach (var obj in collection)
            {
                if (!this.Contains(obj))
                {
                    this.Add(obj);
                }
            }
        }
    }
}
