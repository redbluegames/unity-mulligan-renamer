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
    using NUnit.Framework;

    public class AdjustNumberingOpTests
    {
        [Test]
        public void Rename_NullTarget_ReturnsEmpty()
        {
            // Arrange
            string name = null;
            var renameOp = new AdjustNumberingOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptySequence_AddsNothing()
        {
            // Arrange
            string name = "Blah";
            var renameOp = new AdjustNumberingOperation();
            var expected = new RenameResult() { new Diff("Blah", DiffOperation.Equal) };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_OnlyNumbersPositiveOffset_Adds()
        {
            // Arrange
            var name = "123";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = 1;

            var expected = new RenameResult()
            {
                new Diff("12", DiffOperation.Equal),
                new Diff("3", DiffOperation.Deletion),
                new Diff("4", DiffOperation.Insertion),
            };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_OnlyNumbersNegativeOffset_Subtracts()
        {
            // Arrange
            var name = "123";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = -1;

            var expected = new RenameResult()
            {
                new Diff("12", DiffOperation.Equal),
                new Diff("3", DiffOperation.Deletion),
                new Diff("2", DiffOperation.Insertion),
            };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_BigNegativeOffset_ShortensString()
        {
            // Arrange
            var name = "12";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = -4;

            var expected = new RenameResult()
            {
                new Diff("1", DiffOperation.Deletion),
                new Diff("8", DiffOperation.Insertion),
                new Diff("2", DiffOperation.Deletion),
            };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_BigPositiveOffset_IncreasesStringLength()
        {
            // Arrange
            var name = "1";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = 24;

            var expected = new RenameResult()
            {
                new Diff("1", DiffOperation.Deletion),
                new Diff("25", DiffOperation.Insertion),
            };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_OnlyNumbersZeroOffset_DoesNothing()
        {
            // Arrange
            var name = "123";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = 0;

            var expected = new RenameResult() { new Diff("123", DiffOperation.Equal), };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_MultipleNumberGroups_AddsToAll()
        {
            // Arrange
            var name = "234abc567";
            var renameOp = new AdjustNumberingOperation();
            renameOp.Offset = 1;

            var expected = new RenameResult()
            {
                new Diff("23", DiffOperation.Equal),
                new Diff("4", DiffOperation.Deletion),
                new Diff("5", DiffOperation.Insertion),
                new Diff("abc56", DiffOperation.Equal),
                new Diff("7", DiffOperation.Deletion),
                new Diff("8", DiffOperation.Insertion),
            };

            // Act
            var result = renameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}