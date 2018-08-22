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
    /// RenameOperation for adding a repeating sequence of strings. This is only necessary
    /// because we don't serialize drawers, so in order to figure out which drawer
    /// type corresponds with each operation, we have to make each drawer paired to a single
    /// operation.
    /// </summary>
    [System.Serializable]
    public class AddStringSequenceOperation : CountByLetterOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.AddStringSequenceOperation"/> class.
        /// </summary>
        public AddStringSequenceOperation() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RedBlueGames.MulliganRenamer.AddStringSequenceOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public AddStringSequenceOperation(CountByLetterOperation operationToCopy) : base(operationToCopy) { }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override IRenameOperation Clone()
        {
            var clone = new AddStringSequenceOperation(this);
            return clone;
        }
    }
}