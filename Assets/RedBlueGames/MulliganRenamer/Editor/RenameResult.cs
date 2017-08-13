namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RenameResult : ICollection<Diff>, IEnumerable<Diff>
    {
        public static readonly RenameResult Empty = new RenameResult();

        private List<Diff> diffs;

        public int Count
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public RenameResult()
        {
            this.diffs = new List<Diff>();
        }

        public string NewName
        {
            get
            {
                var name = string.Empty;
                foreach (var diff in this.diffs)
                {
                    if (diff.Operation == DiffOperation.Equal)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                    else if (diff.Operation == DiffOperation.Insertion)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                }

                return name;
            }
        }

        public string OriginalName
        {
            get
            {
                var name = string.Empty;
                foreach (var diff in this.diffs)
                {
                    if (diff.Operation == DiffOperation.Equal)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                    else if (diff.Operation == DiffOperation.Deletion)
                    {
                        name = string.Concat(name, diff.Result);
                    }
                }

                return name;
            }
        }

        public void Clear()
        {
            this.diffs.Clear();
        }

        public bool Contains(Diff item)
        {
            return this.diffs.Contains(item);
        }

        public void CopyTo(Diff[] array, int arrayIndex)
        {
            this.diffs.CopyTo(array, arrayIndex);
        }

        public bool Remove(Diff item)
        {
            return this.diffs.Remove(item);
        }

        public IEnumerator<Diff> GetEnumerator()
        {
            return this.diffs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(Diff diffToAdd)
        {
            this.diffs.Add(diffToAdd);
        }

        public string GetOriginalNameColored(Color32 deletionColor)
        {
            var startTag = string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(deletionColor), ">");
            var endTag = "</color>";
            return this.GetOriginalNameWithTags(startTag, endTag);
        }

        public string GetNewNameColored(Color32 insertionColor)
        {
            var startTag = string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(insertionColor), ">");
            var endTag = "</color>";
            return this.GetNewNameWithTags(startTag, endTag);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherRenameResult = obj as RenameResult;
            if ((System.Object)otherRenameResult == null)
            {
                return false;
            }

            if (this.diffs.Count != otherRenameResult.diffs.Count)
            {
                return false;
            }

            for (int i = 0; i < this.diffs.Count; ++i)
            {
                if (!this.diffs[i].Equals(otherRenameResult.diffs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var renameResultStr = string.Format("[RenameResult: NewName={0}, OriginalName={1}]", NewName, OriginalName);
            var diffs = string.Empty;
            foreach (var diff in this.diffs)
            {
                diffs = string.Concat(diffs, "\n", diff.ToString());
            }

            renameResultStr = string.Concat(renameResultStr, diffs);

            return renameResultStr;
        }

        private string GetOriginalNameWithTags(string deletionTagStart, string deletionTagEnd)
        {
            var name = string.Empty;
            foreach (var diff in this.diffs)
            {
                if (diff.Operation == DiffOperation.Equal)
                {
                    name = string.Concat(name, diff.Result);
                }
                else if (diff.Operation == DiffOperation.Deletion)
                {
                    name = string.Concat(name, deletionTagStart, diff.Result, deletionTagEnd);
                }
            }

            return name;
        }

        private string GetNewNameWithTags(string insertionTagStart, string insertionTagEnd)
        {
            var name = string.Empty;
            foreach (var diff in this.diffs)
            {
                if (diff.Operation == DiffOperation.Equal)
                {
                    name = string.Concat(name, diff.Result);
                }
                else if (diff.Operation == DiffOperation.Insertion)
                {
                    name = string.Concat(name, insertionTagStart, diff.Result, insertionTagEnd);
                }
            }

            return name;
        }
    }
}