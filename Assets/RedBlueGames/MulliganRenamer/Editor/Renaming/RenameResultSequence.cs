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
        /// Gets the name before it changed (or not) at the specified rename step.
        /// </summary>
        /// <returns>The original name at the rename step.</returns>
        /// <param name="stepIndex">Rename step index.</param>
        public RenameResult GetRenameResultBeforeStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= this.NumSteps)
            {
                var exception = string.Format(
                                    LocalizationManager.Instance.GetTranslation("errorTryintToGetOriginalName"),
                                    stepIndex);
                throw new System.ArgumentException(exception, "stepIndex");
            }

            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex);
        }

        /// <summary>
        /// Gets the new name at the specified rename step
        /// </summary>
        /// <returns>The new name at step.</returns>
        /// <param name="stepIndex">Rename step index.</param>
        public RenameResult GetRenameResultForStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= this.NumSteps)
            {
                var exception = string.Format(
                    LocalizationManager.Instance.GetTranslation("errorTryingToGetOriginalNameOutOfBounds"),
                    stepIndex);
                throw new System.ArgumentException(exception, "stepIndex");
            }

            return this.OperationResults.DefaultIfEmpty(RenameResult.Empty).ElementAtOrDefault(stepIndex);
        }
    }
}