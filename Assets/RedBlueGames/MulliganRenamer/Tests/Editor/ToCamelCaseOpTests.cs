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

    public class ToCamelCaseOpTests
    {
        [Test]
        public void Rename_NullTarget_DoesNothing()
        {
            // Arrange
            string name = null;
            var toCamelCaseOp = new ToCamelCaseOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptyString_DoesNothing()
        {
            // Arrange
            var name = string.Empty;
            var toCamelCaseOp = new ToCamelCaseOperation();

            var expected = RenameResult.Empty;

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_SpaceDelimeter_CapitalizesWords()
        {
            // Arrange
            var name = "this is my name";
            var toCamelCaseOp = new ToCamelCaseOperation();
            toCamelCaseOp.DelimiterCharacters = " ";

            var expected = new RenameResult()
            {
                new Diff("t", DiffOperation.Deletion),
                new Diff("T", DiffOperation.Insertion),
                new Diff("his ", DiffOperation.Equal),
                new Diff("i", DiffOperation.Deletion),
                new Diff("I", DiffOperation.Insertion),
                new Diff("s ", DiffOperation.Equal),
                new Diff("m", DiffOperation.Deletion),
                new Diff("M", DiffOperation.Insertion),
                new Diff("y ", DiffOperation.Equal),
                new Diff("n", DiffOperation.Deletion),
                new Diff("N", DiffOperation.Insertion),
                new Diff("ame", DiffOperation.Equal),
            };

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_RegexCharacterAsDelimeter_CapitalizesWords()
        {
            // Arrange
            var name = "this/is/my/name";
            var toCamelCaseOp = new ToCamelCaseOperation();
            toCamelCaseOp.DelimiterCharacters = "/";

            var expected = new RenameResult()
            {
                new Diff("t", DiffOperation.Deletion),
                new Diff("T", DiffOperation.Insertion),
                new Diff("his/", DiffOperation.Equal),
                new Diff("i", DiffOperation.Deletion),
                new Diff("I", DiffOperation.Insertion),
                new Diff("s/", DiffOperation.Equal),
                new Diff("m", DiffOperation.Deletion),
                new Diff("M", DiffOperation.Insertion),
                new Diff("y/", DiffOperation.Equal),
                new Diff("n", DiffOperation.Deletion),
                new Diff("N", DiffOperation.Insertion),
                new Diff("ame", DiffOperation.Equal),
            };

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_MultipleDelimeters_CapitalizesWords()
        {
            // Arrange
            var name = "this-is my_name";
            var toCamelCaseOp = new ToCamelCaseOperation();
            toCamelCaseOp.DelimiterCharacters = "- _";

            var expected = new RenameResult()
            {
                new Diff("t", DiffOperation.Deletion),
                new Diff("T", DiffOperation.Insertion),
                new Diff("his-", DiffOperation.Equal),
                new Diff("i", DiffOperation.Deletion),
                new Diff("I", DiffOperation.Insertion),
                new Diff("s ", DiffOperation.Equal),
                new Diff("m", DiffOperation.Deletion),
                new Diff("M", DiffOperation.Insertion),
                new Diff("y_", DiffOperation.Equal),
                new Diff("n", DiffOperation.Deletion),
                new Diff("N", DiffOperation.Insertion),
                new Diff("ame", DiffOperation.Equal),
            };

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_CharacterDelimiter_DelimitersArentCapitalized()
        {
            // Arrange
            var name = "thisxisxmyxname";
            var toCamelCaseOp = new ToCamelCaseOperation();
            toCamelCaseOp.DelimiterCharacters = "x";

            var expected = new RenameResult()
            {
                new Diff("t", DiffOperation.Deletion),
                new Diff("T", DiffOperation.Insertion),
                new Diff("hisx", DiffOperation.Equal),
                new Diff("i", DiffOperation.Deletion),
                new Diff("I", DiffOperation.Insertion),
                new Diff("sx", DiffOperation.Equal),
                new Diff("m", DiffOperation.Deletion),
                new Diff("M", DiffOperation.Insertion),
                new Diff("yx", DiffOperation.Equal),
                new Diff("n", DiffOperation.Deletion),
                new Diff("N", DiffOperation.Insertion),
                new Diff("ame", DiffOperation.Equal),
            };

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptyDelimeters_RemainsUnchanged()
        {
            // Arrange
            var name = "whats in a name";
            var toCamelCaseOp = new ToCamelCaseOperation();
            toCamelCaseOp.DelimiterCharacters = string.Empty;

            var expected = new RenameResult()
            {
                new Diff(name, DiffOperation.Equal)
            };

            // Act
            var result = toCamelCaseOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}