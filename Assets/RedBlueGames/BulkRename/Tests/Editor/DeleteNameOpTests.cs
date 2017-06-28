namespace RedBlueGames.BulkRename.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class DeleteNameOpTests
    {
        [Test]
        public void Rename_NameIsEmpty_IsUnchanged()
        {
            // Arrange
            var name = string.Empty;
            var deleteCharactersOp = new DeleteNameOperation();

            var expected = string.Empty;

            // Act
            string result = deleteCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_NameIsNotEmpty_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var deleteCharactersOp = new DeleteNameOperation();

            var expected = string.Empty;

            // Act
            string result = deleteCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}