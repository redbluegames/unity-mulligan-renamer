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

    public class RemoveCharactersOpTests
    {
        [Test]
        public void RemoveCharacters_NullTarget_IsUnchanged()
        {
            // Arrange
            string name = null;
            var removeCharactersOp = new RemoveCharactersOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCharacters_EmptyTarget_IsUnchanged()
        {
            // Arrange
            var name = string.Empty;
            var removeCharactersOp = new RemoveCharactersOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveSymbols_SymbolsAndAlphanumericsInString_RemovesOnlySymbols()
        {
            // Arrange
            var name = "A!@#$%BD*(";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Symbols;

            var expected = new RenameResult()
            {
                new Diff("A", DiffOperation.Equal),
                new Diff("!", DiffOperation.Deletion),
                new Diff("@", DiffOperation.Deletion),
                new Diff("#", DiffOperation.Deletion),
                new Diff("$", DiffOperation.Deletion),
                new Diff("%", DiffOperation.Deletion),
                new Diff("BD", DiffOperation.Equal),
                new Diff("*", DiffOperation.Deletion),
                new Diff("(", DiffOperation.Deletion),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveSymbols_OnlySymbolsInString_RemovesAllSymbols()
        {
            // Arrange
            var name = "`~!@#$%^&*()+-=[]{}\\|;:'\",<.>/?";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Symbols;

            var expected = new RenameResult();
            for (int i = 0; i < name.Length; ++i)
            {
                var substring = name.Substring(i, 1);
                expected.Add(new Diff(substring, DiffOperation.Deletion));
            }

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveSymbols_StringHasWhitespaceAndSymbols_KeepsWhitespace()
        {
            // Arrange
            var name = "!A !";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Symbols;

            var expected = new RenameResult()
            {
                new Diff("!", DiffOperation.Deletion),
                new Diff("A ", DiffOperation.Equal),
                new Diff("!", DiffOperation.Deletion),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveNumbers_LettersAndNumbersInString_RemovesOnlyNumbers()
        {
            // Arrange
            var name = "A251B637k911p";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Numbers;

            var expected = new RenameResult()
            {
                new Diff("A", DiffOperation.Equal),
                new Diff("2", DiffOperation.Deletion),
                new Diff("5", DiffOperation.Deletion),
                new Diff("1", DiffOperation.Deletion),
                new Diff("B", DiffOperation.Equal),
                new Diff("6", DiffOperation.Deletion),
                new Diff("3", DiffOperation.Deletion),
                new Diff("7", DiffOperation.Deletion),
                new Diff("k", DiffOperation.Equal),
                new Diff("9", DiffOperation.Deletion),
                new Diff("1", DiffOperation.Deletion),
                new Diff("1", DiffOperation.Deletion),
                new Diff("p", DiffOperation.Equal),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveNumbers_AllNumbersInString_RemovesAllNumbers()
        {
            // Arrange
            var name = "1234567890";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Numbers;

            var expected = new RenameResult();
            for (int i = 0; i < name.Length; ++i)
            {
                var substring = name.Substring(i, 1);
                expected.Add(new Diff(substring, DiffOperation.Deletion));
            }

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveWhitespace_LettersSymbolsAndWhitespaceInString_RemovesOnlyWhitespace()
        {
            // Arrange
            var name = "A1! B";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Whitespace;

            var expected = new RenameResult()
            {
                new Diff("A1!", DiffOperation.Equal),
                new Diff(" ", DiffOperation.Deletion),
                new Diff("B", DiffOperation.Equal),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveWhitespace_AllWhitespace_IsEmpty()
        {
            // Arrange
            var name = "    ";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = RemoveCharactersOperation.Whitespace;

            var expected = new RenameResult();
            for (int i = 0; i < name.Length; ++i)
            {
                var substring = name.Substring(i, 1);
                expected.Add(new Diff(substring, DiffOperation.Deletion));
            }

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCustomCharacters_ValidString_RemovesCustomChars()
        {
            // Arrange
            var name = "abz35!450k";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = new RemoveCharactersOperation.Configuration()
            {
                CharactersToRemove = "ak!5"
            };

            var expected = new RenameResult()
            {
                new Diff("a", DiffOperation.Deletion),
                new Diff("bz3", DiffOperation.Equal),
                new Diff("5", DiffOperation.Deletion),
                new Diff("!", DiffOperation.Deletion),
                new Diff("4", DiffOperation.Equal),
                new Diff("5", DiffOperation.Deletion),
                new Diff("0", DiffOperation.Equal),
                new Diff("k", DiffOperation.Deletion),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveCustomCaseSensitiveCharacters_MixedCasesInString_RemovesCasedCustomChars()
        {
            // Arrange
            var name = "ABCDabcdD";
            var removeCharactersOp = new RemoveCharactersOperation();
            removeCharactersOp.Config = new RemoveCharactersOperation.Configuration()
            {
                CharactersToRemove = "ABCD",
                IsCaseSensitive = true
            };

            var expected = new RenameResult()
            {
                new Diff("A", DiffOperation.Deletion),
                new Diff("B", DiffOperation.Deletion),
                new Diff("C", DiffOperation.Deletion),
                new Diff("D", DiffOperation.Deletion),
                new Diff("abcd", DiffOperation.Equal),
                new Diff("D", DiffOperation.Deletion),
            };

            // Act
            var result = removeCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}