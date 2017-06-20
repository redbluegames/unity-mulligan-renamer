namespace RedBlueGames.BulkRename.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class RemoveCharactersOpTests
    {
        [Test]
        public void RemoveCharacters_EmptyTarget_IsUnchanged()
        {
            // Arrange
            var name = string.Empty;
            var removeCharactersOp = new RemoveCharactersOperation();

            var expected = string.Empty;

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCharacters_RemoveSymbols_RemovesOnlySymbols()
        {
            // Arrange
            var name = "A!@#$%BD*(";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Characters = "!@#$%^&*()";

            var expected = "ABD";

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}