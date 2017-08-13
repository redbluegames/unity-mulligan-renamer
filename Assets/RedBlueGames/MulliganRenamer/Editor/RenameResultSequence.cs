namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Rename result sequence contains the data for the results of a BulkRename operation.
    /// </summary>
    public class RenameResultSequence
    {
        public RenameResultSequence(List<RenameResult> renameOperationResults)
        {
            this.OperationResults = renameOperationResults;
        }

        private List<RenameResult> OperationResults { get; set; }

        /// <summary>
        /// Gets the original name, before any rename operation.
        /// </summary>
        /// <value>The original name.</value>
        public string OriginalName
        {
            get
            {
                return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).First().OriginalName;
            }
        }

        /// <summary>
        /// Gets the new name, after the rename operations.
        /// </summary>
        /// <value>The new name.</value>
        public string NewName
        {
            get
            {
                return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).Last().NewName;
            }
        }

        public string GetOriginalNameAtStep(int stepIndex)
        {
            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex).GetOriginalNameColored(Color.red);
        }

        public string GetNewNameAtStep(int stepIndex)
        {
            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex).GetNewNameColored(Color.green);
        }

        public int NumSteps
        {
            get
            {
                return this.OperationResults.Count;
            }
        }
    }
}