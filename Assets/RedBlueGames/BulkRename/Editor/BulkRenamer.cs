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

namespace RedBlueGames.BulkRename
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Bulk renamer handles configuration and renaming of names.
    /// </summary>
    public class BulkRenamer
    {
        private List<IRenameOperation> renameOperations;

        private List<IRenameOperation> RenameOperations
        {
            get
            {
                if (this.renameOperations == null)
                {
                    this.renameOperations = new List<IRenameOperation>();
                }

                return this.renameOperations;
            }
        }

        /// <summary>
        /// Sets the rename operation to use.
        /// </summary>
        /// <param name="operation">Operation to assign.</param>
        public void SetRenameOperation(IRenameOperation operation)
        {
            var operationList = new List<IRenameOperation>();
            operationList.Add(operation);
            this.SetRenameOperations(operationList);
        }

        /// <summary>
        /// Sets the rename operations.
        /// </summary>
        /// <param name="operations">Operations to assign.</param>
        public void SetRenameOperations(List<IRenameOperation> operations)
        {
            this.RenameOperations.Clear();
            this.RenameOperations.AddRange(operations);
        }

        /// <summary>
        /// Gets all strings renamed according to the BulkRenamer configuration.
        /// </summary>
        /// <returns>The renamed strings.</returns>
        /// <param name="includeDiff">If set to <c>true</c> outputs the name including diff with rich text tags.</param>
        /// <param name="originalNames">Original names to rename.</param>
        public string[] GetRenamedStrings(bool includeDiff, params string[] originalNames)
        {
            var renamedStrings = new string[originalNames.Length];

            for (int i = 0; i < originalNames.Length; ++i)
            {
                renamedStrings[i] = this.GetRenamedString(originalNames[i], i, includeDiff);
            }

            return renamedStrings;
        }

        private string GetRenamedString(string originalName, int count, bool includeDiff)
        {
            var modifiedName = originalName;

            foreach (var op in this.RenameOperations)
            {
                modifiedName = op.Rename(modifiedName, count, includeDiff);
            }

            return modifiedName;
        }
    }
}