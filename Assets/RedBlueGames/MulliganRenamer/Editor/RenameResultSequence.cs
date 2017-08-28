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
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameResultSequence"/> class.
        /// </summary>
        /// <param name="renameOperationResults">Rename operation results.</param>
        public RenameResultSequence(List<RenameResult> renameOperationResults)
        {
            this.OperationResults = renameOperationResults;
        }

        /// <summary>
        /// Gets the original name, before any rename operation.
        /// </summary>
        /// <value>The original name.</value>
        public string OriginalName
        {
            get
            {
                return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).First().Original;
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
                return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).Last().Result;
            }
        }

        /// <summary>
        /// Gets the number of steps in the sequence.
        /// </summary>
        /// <value>The number of steps.</value>
        public int NumSteps
        {
            get
            {
                return this.OperationResults.Count;
            }
        }

        private List<RenameResult> OperationResults { get; set; }

        /// <summary>
        /// Gets the original name at the specified rename step.
        /// </summary>
        /// <returns>The original name at the rename step.</returns>
        /// <param name="stepIndex">Rename step index.</param>
        public string GetOriginalNameAtStep(int stepIndex)
        {
            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex).GetOriginalColored(Color.red);
        }

        /// <summary>
        /// Gets the new name at the specified rename step
        /// </summary>
        /// <returns>The new name at step.</returns>
        /// <param name="stepIndex">Rename step index.</param>
        public string GetNewNameAtStep(int stepIndex)
        {
            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex).GetResultColored(Color.green);
        }
    }
}