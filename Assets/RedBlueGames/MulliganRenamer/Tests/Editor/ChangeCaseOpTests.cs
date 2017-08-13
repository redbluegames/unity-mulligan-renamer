namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class ChangeCaseOpTests
    {
        [Test]
        public void Rename_NullTarget_DoesNothing()
        {
            // Arrange
            string name = null;
            var changeCaseOp = new ChangeCaseOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptyString_DoesNothing()
        {
            // Arrange
            var name = string.Empty;
            var changeCaseOp = new ChangeCaseOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameToLower_ValidUpperCharacters_AreLowered()
        {
            // Arrange
            var name = "SOME UPPER";
            var changeCaseOp = new ChangeCaseOperation();

            var expectedName = "some upper";
            var expected = new RenameResult();
            for (int i = 0; i < name.Length; ++i)
            {
                var expectedNameChar = expectedName.Substring(i, 1);
                var nameChar = name.Substring(i, 1);
                if (nameChar == expectedNameChar)
                {
                    expected.Add(new Diff(nameChar, DiffOperation.Equal));
                    continue;
                }

                expected.Add(new Diff(nameChar, DiffOperation.Deletion));
                expected.Add(new Diff(expectedNameChar, DiffOperation.Insertion));
            }

            // Act
            var result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameToUpper_ValidLowerCharacters_AreUppered()
        {
            // Arrange
            var name = "this is all lower";
            var changeCaseOp = new ChangeCaseOperation();
            changeCaseOp.ToUpper = true;

            var expectedName = "THIS IS ALL LOWER";
            var expected = new RenameResult();
            for (int i = 0; i < name.Length; ++i)
            {
                var expectedNameChar = expectedName.Substring(i, 1);
                var nameChar = name.Substring(i, 1);
                if (nameChar == expectedNameChar)
                {
                    expected.Add(new Diff(nameChar, DiffOperation.Equal));
                    continue;
                }

                expected.Add(new Diff(nameChar, DiffOperation.Deletion));
                expected.Add(new Diff(expectedNameChar, DiffOperation.Insertion));
            }

            // Act
            var result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_Symbols_AreUnchanged()
        {
            // Arrange
            var name = "!@#$%^&*()_-=+[]\\;',.";
            var changeCaseOp = new ChangeCaseOperation();

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}