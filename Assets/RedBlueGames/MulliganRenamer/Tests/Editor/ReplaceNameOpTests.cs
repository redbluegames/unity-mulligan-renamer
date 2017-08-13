namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class ReplaceNameOpTests
    {
        [Test]
        public void Rename_DeleteTargetNameIsNull_Renames()
        {
            // Arrange
            string name = null;
            var replaceNameOp = new ReplaceNameOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_DeleteTargetNameIsEmpty_IsUnchanged()
        {
            // Arrange
            var name = string.Empty;
            var replaceNameOp = new ReplaceNameOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_DeleteTargetNameIsNotEmpty_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var replaceNameOp = new ReplaceNameOperation();

            var expected = new RenameResult() { new Diff(name, DiffOperation.Deletion) };

            // Act
            var result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_ReplaceName_IsReplaced()
        {
            // Arrange
            var name = "Char_Hero";
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "Blah";

            var expected = new RenameResult()
            {
                new Diff(name, DiffOperation.Deletion),
                new Diff("Blah", DiffOperation.Insertion)
            };

            // Act
            var result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}