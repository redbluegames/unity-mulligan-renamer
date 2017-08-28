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

    public class ReplaceStringOpTests
    {
        [Test]
        public void SearchString_NullTarget_DoesNothing()
        {
            // Arrange
            string name = null;
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Anything";

            var expected = RenameResult.Empty;

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_EmptyTarget_DoesNothing()
        {
            // Arrange
            var name = string.Empty;
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Anything";

            var expected = RenameResult.Empty;

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_EmptySearch_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = string.Empty;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_EmptySearch_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = string.Empty;

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_OneMatch_IsReplaced()
        {
            // Arrange
            var name = "CHAR_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "A";

            var expected = new RenameResult();
            expected.Add(new Diff("CHAR_", DiffOperation.Equal));
            expected.Add(new Diff("Hero", DiffOperation.Deletion));
            expected.Add(new Diff("A", DiffOperation.Insertion));
            expected.Add(new Diff("_Spawn", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_OneMatch_IsReplaced()
        {
            // Arrange
            var name = "CHAR_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "A";

            var expected = new RenameResult();
            expected.Add(new Diff("CHAR_", DiffOperation.Equal));
            expected.Add(new Diff("Hero", DiffOperation.Deletion));
            expected.Add(new Diff("A", DiffOperation.Insertion));
            expected.Add(new Diff("_Spawn", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_PartialMatches_AreNotReplaced()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Heroine";

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_PartialMatches_AreNotReplaced()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "Heroine";

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_MultipleMatches_AllAreReplaced()
        {
            // Arrange
            var name = "StoolDoodad";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "o";

            var expected = new RenameResult();
            expected.Add(new Diff("St", DiffOperation.Equal));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("lD", DiffOperation.Equal));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("dad", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_MultipleMatches_AllAreReplaced()
        {
            // Arrange
            var name = "StoolDoodad";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "o";

            var expected = new RenameResult();
            expected.Add(new Diff("St", DiffOperation.Equal));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("lD", DiffOperation.Equal));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("o", DiffOperation.Deletion));
            expected.Add(new Diff("dad", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_NotCaseSensitiveAndDoesNotMatchCase_StillReplaces()
        {
            // Arrange
            var name = "ZELDAzelda";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = false;
            replaceStringOp.SearchString = "ZelDa";
            replaceStringOp.ReplacementString = "blah";

            var expected = new RenameResult();
            expected.Add(new Diff("ZELDA", DiffOperation.Deletion));
            expected.Add(new Diff("blah", DiffOperation.Insertion));
            expected.Add(new Diff("zelda", DiffOperation.Deletion));
            expected.Add(new Diff("blah", DiffOperation.Insertion));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void SearchStringCaseSensitive_DoesNotMatchCase_DoesNotReplace()
        {
            // Arrange
            var name = "ZELDA";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "zelda";
            replaceStringOp.ReplacementString = "blah";

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchStringCaseSensitive_MatchesCase_Replaces()
        {
            // Arrange
            var name = "ZeldA";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "ZeldA";
            replaceStringOp.ReplacementString = "blah";

            var expected = new RenameResult();
            expected.Add(new Diff("ZeldA", DiffOperation.Deletion));
            expected.Add(new Diff("blah", DiffOperation.Insertion));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_SpecialCharactersInSearch_Replaces()
        {
            // Arrange
            var name = "Char_Hero_Woot";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "[a-zA-Z]*_";
            replaceStringOp.ReplacementString = "Yep";

            var expected = new RenameResult();
            expected.Add(new Diff("Char_", DiffOperation.Deletion));
            expected.Add(new Diff("Yep", DiffOperation.Insertion));
            expected.Add(new Diff("Hero_", DiffOperation.Deletion));
            expected.Add(new Diff("Yep", DiffOperation.Insertion));
            expected.Add(new Diff("Woot", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_EscapeCharactersInSearch_Replaces()
        {
            // Arrange
            var name = "Char.Hero.Woot";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "\\.";
            replaceStringOp.ReplacementString = "_";

            var expected = new RenameResult();
            expected.Add(new Diff("Char", DiffOperation.Equal));
            expected.Add(new Diff(".", DiffOperation.Deletion));
            expected.Add(new Diff("_", DiffOperation.Insertion));
            expected.Add(new Diff("Hero", DiffOperation.Equal));
            expected.Add(new Diff(".", DiffOperation.Deletion));
            expected.Add(new Diff("_", DiffOperation.Insertion));
            expected.Add(new Diff("Woot", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_ReplaceNeighboringMatches_SubstitutesMultiple()
        {
            // Arrange
            var name = "Thisisyours";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "is";
            replaceStringOp.ReplacementString = "e";

            var expected = new RenameResult();
            expected.Add(new Diff("Th", DiffOperation.Equal));
            expected.Add(new Diff("is", DiffOperation.Deletion));
            expected.Add(new Diff("e", DiffOperation.Insertion));
            expected.Add(new Diff("is", DiffOperation.Deletion));
            expected.Add(new Diff("e", DiffOperation.Insertion));
            expected.Add(new Diff("yours", DiffOperation.Equal));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_ReplaceSeparatedMatches_SubstitutesMultiple()
        {
            // Arrange
            var name = "You do you";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "you";
            replaceStringOp.ReplacementString = "Me";

            var expected = new RenameResult();
            expected.Add(new Diff("You", DiffOperation.Deletion));
            expected.Add(new Diff("Me", DiffOperation.Insertion));
            expected.Add(new Diff(" do ", DiffOperation.Equal));
            expected.Add(new Diff("you", DiffOperation.Deletion));
            expected.Add(new Diff("Me", DiffOperation.Insertion));

            // Act
            var result = replaceStringOp.Rename(name, 0);

            Assert.AreEqual(expected, result);
        }
    }
}