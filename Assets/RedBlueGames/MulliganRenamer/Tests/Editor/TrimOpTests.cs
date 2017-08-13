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