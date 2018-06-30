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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class AddStringSequenceOpTests
    {
        [Test]
        public void Clone_ValidOther_ClonesAllProperties()
        {
            // Arrange
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A", "B", "C" };

            // Act
            var result = new AddStringSequenceOperation(addStringSequenceOp);

            // Assert
            Assert.AreEqual(addStringSequenceOp.StringSequence, result.StringSequence);
        }

        [Test]
        public void Rename_NoSuppliedSequence_DoesNothing()
        {
            // Arrange
            string name = "Entry";
            var addStringSequenceOp = new AddStringSequenceOperation();

            var expected = new RenameResult() { new Diff("Entry", DiffOperation.Equal) };

            // Act
            var result = addStringSequenceOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptySequence_DoesNothing()
        {
            // Arrange
            string name = "Entry";
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[0];

            var expected = new RenameResult() { new Diff("Entry", DiffOperation.Equal) };

            // Act
            var result = addStringSequenceOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_SequenceWithEmptyStrings_DoesNothing()
        {
            // Arrange
            string name = "Entry";
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { string.Empty };

            var expected = new RenameResult() { new Diff("Entry", DiffOperation.Equal) };

            // Act
            var result = addStringSequenceOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_NullTarget_AddsSequence()
        {
            // Arrange
            string name = null;
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A" };

            var expected = new RenameResult() { new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = addStringSequenceOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_SequenceIntoEmpty_AddsSequence()
        {
            // Arrange
            string name = string.Empty;
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A" };

            var expected = new RenameResult() { new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = addStringSequenceOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_AddMiddleStringFromSequence_AddsSpecifiedString()
        {
            // Arrange
            string name = "Boop";
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A", "B", "C" };

            var expected = new RenameResult() {
                new Diff("Boop", DiffOperation.Equal),
                new Diff("B", DiffOperation.Insertion)
            };

            // Act
            var result = addStringSequenceOp.Rename(name, 1);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_AddLastStringFromSequence_AddsSpecifiedString()
        {
            // Arrange
            string name = null;
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A", "B", "C" };

            var expected = new RenameResult() { new Diff("C", DiffOperation.Insertion) };

            // Act
            var result = addStringSequenceOp.Rename(name, 2);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_SpecifiedCountIsHigherThanSequenceCount_WrapsToIndex()
        {
            // Arrange
            string name = null;
            var addStringSequenceOp = new AddStringSequenceOperation();
            addStringSequenceOp.StringSequence = new string[] { "A", "B", "C" };

            var expected = new RenameResult() { new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = addStringSequenceOp.Rename(name, 3);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}