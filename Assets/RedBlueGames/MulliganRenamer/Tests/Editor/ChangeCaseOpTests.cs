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
            changeCaseOp.Casing = ChangeCaseOperation.CasingChange.Uppercase;

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