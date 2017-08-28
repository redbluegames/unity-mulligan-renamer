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