namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class ReplaceStringOpTests
    {
        [Test]
        public void SearchString_Empty_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = string.Empty;

            var expected = name;

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "CHAR_A_Spawn";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "StlDdad";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = name;

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_Replacement_SubstitutesForSearchString()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "Link";

            var expected = "Char_Link_Spawn";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_DoesNotMatchCase_Replaces()
        {
            // Arrange
            var name = "ZELDAzelda";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = false;
            replaceStringOp.SearchString = "ZelDa";
            replaceStringOp.ReplacementString = "blah";

            var expected = "blahblah";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "ZELDA";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "blah";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_Empty_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = string.Empty;

            var expected = name;

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "CHAR_A_Spawn";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "StlDdad";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = name;

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_Replacement_SubstitutesForSearchString()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "Link";

            var expected = "Char_Link_Spawn";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_DoesNotMatchCase_Replaces()
        {
            // Arrange
            var name = "ZELDAzelda";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = false;
            replaceStringOp.SearchString = "ZelDa";
            replaceStringOp.ReplacementString = "blah";

            var expected = "blahblah";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegexCaseSensitive_DoesNotMatchCase_DoesNotReplace()
        {
            // Arrange
            var name = "ZELDA";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "zelda";
            replaceStringOp.ReplacementString = "blah";

            var expected = "ZELDA";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegexCaseSensitive_MatchesCase_Replaces()
        {
            // Arrange
            var name = "ZeldA";
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "ZeldA";
            replaceStringOp.ReplacementString = "blah";

            var expected = "blah";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "YepYepWoot";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = "Char_Hero_Woot";

            // Act
            string result = replaceStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}