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

    public class AddStringOpTests
    {
        [Test]
        public void AddPrefix_NullTarget_Adds()
        {
            // Arrange
            string name = null;
            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = "Pre";

            var expected = new RenameResult() { new Diff("Pre", DiffOperation.Insertion) };

            // Act
            var result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddPrefix_Empty_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = string.Empty;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddPrefix_ValidPrefix_IsAdded()
        {
            // Arrange
            var name = "Hero_Spawn";
            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = "Char_";

            var expected = new RenameResult()
            {
                new Diff("Char_", DiffOperation.Insertion),
                new Diff("Hero_Spawn", DiffOperation.Equal)
            };

            // Act
            var result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddSuffix_Empty_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var addStringOp = new AddStringOperation();
            addStringOp.Suffix = string.Empty;

            var expected = new RenameResult() { new Diff("Char_Hero_Spawn", DiffOperation.Equal) };

            // Act
            var result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddSuffix_ValidSuffix_IsAdded()
        {
            // Arrange
            var name = "Char_Hero";
            var addStringOp = new AddStringOperation();
            addStringOp.Suffix = "_Spawn";

            var expected = new RenameResult()
            {
                new Diff("Char_Hero", DiffOperation.Equal),
                new Diff("_Spawn", DiffOperation.Insertion)
            };

            // Act
            var result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}