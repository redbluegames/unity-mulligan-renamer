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
        public void RemoveSymbols_SymbolsAndAlphanumericsInString_RemovesOnlySymbols()
        {
            // Arrange
            var name = "A!@#$%BD*(";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = RemoveCharactersOperation.Symbols;

            var expected = "ABD";

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveSymbols_OnlySymbolsInString_RemovesAllSymbols()
        {
            // Arrange
            var name = "`~!@#$%^&*()+-=[]{}\\|;:'\",<.>/?";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = RemoveCharactersOperation.Symbols;

            var expected = string.Empty;

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveNumbers_LettersAndNumbersInString_RemovesOnlyNumbers()
        {
            // Arrange
            var name = "A251B637k911p";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = RemoveCharactersOperation.Numbers;

            var expected = "ABkp";

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveNumbers_AllNumbersInString_RemovesAllNumbers()
        {
            // Arrange
            var name = "1234567890";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = RemoveCharactersOperation.Numbers;

            var expected = string.Empty;

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCustomCharacters_ValidString_RemovesCustomChars()
        {
            // Arrange
            var name = "abz35!450k";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = new RemoveCharactersOperation.RemoveCharactersOperationOptions()
            {
                CharactersToRemove = "ak!5"
            };

            var expected = "bz340";

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCustomCaseSensitiveCharacters_MixedCasesInString_RemovesCasedCustomChars()
        {
            // Arrange
            var name = "ABCDabcdD";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Options = new RemoveCharactersOperation.RemoveCharactersOperationOptions()
            {
                CharactersToRemove = "ABCD",
                IsCaseSensitive = true
            };

            var expected = "abcd";

            // Act
            string result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}