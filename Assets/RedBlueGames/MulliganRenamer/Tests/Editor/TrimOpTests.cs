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

    public class TrimOpTests
    {
        [Test]
        public void Trimming_TargetIsNull_IsEmpty()
        {
            // Arrange
            string name = null;
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;

            var expected = RenameResult.Empty;

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNone_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromFront_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = new RenameResult()
            {
                new Diff("C", DiffOperation.Deletion),
                new Diff("har_Hero", DiffOperation.Equal),
            };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 1;

            var expected = new RenameResult()
            {
                new Diff("Char_Her", DiffOperation.Equal),
                new Diff("o", DiffOperation.Deletion),
            };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteFromFrontAndBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 1;

            var expected = new RenameResult()
            {
                new Diff("C", DiffOperation.Deletion),
                new Diff("har_Her", DiffOperation.Equal),
                new Diff("o", DiffOperation.Deletion),
            };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteLongerThanString_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length + 2;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Deletion) };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteTooManyFromBothDirections_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length;
            trimCharactersOp.NumBackDeleteChars = name.Length;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Deletion) };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNegative_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = -1;
            trimCharactersOp.NumBackDeleteChars = -10;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}