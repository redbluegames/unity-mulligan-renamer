namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class RenameOperationSequence<T> : IList<T> where T : IRenameOperation
    {
        private List<T> operationSequence;

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public RenameOperationSequence()
        {
            this.operationSequence = new List<T>();
        }

        public T this [int index]
        {
            get
            {
                return this.operationSequence[index];
            }

            set
            {
                this.operationSequence[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return this.operationSequence.Count;
            }
        }

        public void Add(T item)
        {
            this.operationSequence.Add(item);
        }

        public int IndexOf(T item)
        {
            return this.operationSequence.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.operationSequence.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.operationSequence.RemoveAt(index);
        }

        public void Clear()
        {
            this.operationSequence.Clear();
        }

        public bool Contains(T item)
        {
            return this.operationSequence.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.operationSequence.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this.operationSequence.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void MoveRenameOpFromIndexToIndex(int fromIndex, int desiredIndex)
        {
            desiredIndex = Mathf.Clamp(desiredIndex, 0, this.operationSequence.Count - 1);
            var destinationElementCopy = this.operationSequence[desiredIndex];
            this.operationSequence[desiredIndex] = this.operationSequence[fromIndex];
            this.operationSequence[fromIndex] = destinationElementCopy;
        }

        public RenameResultSequence GetRenamePreview(string originalName, int count)
        {
            var renameResults = this.GetRenameSequenceForName(originalName, count);
            var resultSequence = new RenameResultSequence(renameResults);

            return resultSequence;
        }

        public string GetResultingName(string originalName, int count)
        {
            var resultSequence = GetRenamePreview(originalName, count);
            return resultSequence.NewName;
        }

        private List<RenameResult> GetRenameSequenceForName(string originalName, int count)
        {
            var renameResults = new List<RenameResult>();
            string modifiedName = originalName;
            RenameResult result;

            if (this.operationSequence.Count == 0)
            {
                result = new RenameResult();
                result.Add(new Diff(originalName, DiffOperation.Equal));
                renameResults.Add(result);
            }
            else
            {
                foreach (var op in this.operationSequence)
                {
                    result = op.Rename(modifiedName, count);
                    renameResults.Add(result);
                    modifiedName = result.Result;
                }
            }

            return renameResults;
        }
    }
}