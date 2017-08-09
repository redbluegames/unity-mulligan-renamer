namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class ReplaceNameOpTests
    {
        [Test]
        public void Rename_NameIsNull_Renames()
        {
            // Arrange
            string name = null;
            var replaceNameOp = new ReplaceNameOperation();

            var expected = string.Empty;

            // Act
            string result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_NameIsEmpty_IsUnchanged()
        {
            // Arrange
            var name = string.Empty;
            var replaceNameOp = new ReplaceNameOperation();

            var expected = string.Empty;

            // Act
            string result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_NameIsNotEmpty_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var replaceNameOp = new ReplaceNameOperation();

            var expected = string.Empty;

            // Act
            string result = replaceNameOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}