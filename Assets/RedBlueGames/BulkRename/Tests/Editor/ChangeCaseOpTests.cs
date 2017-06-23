namespace RedBlueGames.BulkRename.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class ChangeCaseOpTests
    {
        [Test]
        public void Rename_EmptyString_DoesNothing()
        {
            // Arrange
            var name = string.Empty;
            var changeCaseOp = new ChangeCaseOperation();

            var expected = string.Empty;

            // Act
            string result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameToLower_ValidUpperCharacters_AreLowered()
        {
            // Arrange
            var name = "THIS IS ALL UPPER";
            var changeCaseOp = new ChangeCaseOperation();

            var expected = "this is all upper";

            // Act
            string result = changeCaseOp.Rename(name, 0);

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

            var expected = "THIS IS ALL LOWER";

            // Act
            string result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_Symbols_AreUnchanged()
        {
            // Arrange
            var name = "!@#$%^&*()_-=+[]\\;',.";
            var changeCaseOp = new ChangeCaseOperation();

            var expected = name;

            // Act
            string result = changeCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}