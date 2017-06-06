namespace RedBlueGames.BulkRename.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class BulkRenamerUnitTests
    {
        [Test]
        public void SearchString_Empty_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = string.Empty;
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_OneMatch_IsReplaced()
        {
            // Arrange
            var name = "CHAR_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "A";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "CHAR_A_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_MultipleMatches_AllAreReplaced()
        {
            // Arrange
            var name = "StoolDoodad";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "o";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "StlDdad";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_PartialMatches_AreNotReplaced()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Heroine";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_Replacement_SubstitutesForSearchString()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Hero";
            replaceStringOp.ReplacementString = "Link";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "Char_Link_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchString_DoesNotMatchCase_Replaces()
        {
            // Arrange
            var objectName = "ZELDAzelda";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = false;
            replaceStringOp.SearchString = "ZelDa";
            replaceStringOp.ReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "blahblah";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchStringCaseSensitive_DoesNotMatchCase_DoesNotReplace()
        {
            // Arrange
            var objectName = "ZELDA";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "zelda";
            replaceStringOp.ReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "ZELDA";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchStringCaseSensitive_MatchesCase_Replaces()
        {
            // Arrange
            var objectName = "ZeldA";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.SearchString = "ZeldA";
            replaceStringOp.ReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "blah";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_Empty_DoesNothing()
        {
            // Arrange
            var name = "ThisIsAName";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = string.Empty;
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_OneMatch_IsReplaced()
        {
            // Arrange
            var name = "CHAR_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "Hero";
            replaceStringOp.RegexReplacementString = "A";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "CHAR_A_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_MultipleMatches_AllAreReplaced()
        {
            // Arrange
            var name = "StoolDoodad";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "o";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "StlDdad";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_PartialMatches_AreNotReplaced()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "Heroine";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_Replacement_SubstitutesForSearchString()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "Hero";
            replaceStringOp.RegexReplacementString = "Link";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "Char_Link_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_DoesNotMatchCase_Replaces()
        {
            // Arrange
            var objectName = "ZELDAzelda";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = false;
            replaceStringOp.RegexSearchString = "ZelDa";
            replaceStringOp.RegexReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "blahblah";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegexCaseSensitive_DoesNotMatchCase_DoesNotReplace()
        {
            // Arrange
            var objectName = "ZELDA";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.RegexSearchString = "zelda";
            replaceStringOp.RegexReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "ZELDA";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegexCaseSensitive_MatchesCase_Replaces()
        {
            // Arrange
            var objectName = "ZeldA";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.SearchIsCaseSensitive = true;
            replaceStringOp.RegexSearchString = "ZeldA";
            replaceStringOp.RegexReplacementString = "blah";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "blah";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_SpecialCharactersInSearch_Replaces()
        {
            // Arrange
            var objectName = "Char_Hero_Woot";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "[a-zA-Z]*_";
            replaceStringOp.RegexReplacementString = "Yep";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "YepYepWoot";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SearchRegex_EscapeCharactersInSearch_Replaces()
        {
            // Arrange
            var objectName = "Char.Hero.Woot";
            var bulkRenamer = new BulkRenamer();
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.UseRegex = true;
            replaceStringOp.RegexSearchString = "\\.";
            replaceStringOp.RegexReplacementString = "_";
            bulkRenamer.SetRenameOperation(replaceStringOp);

            var expected = "Char_Hero_Woot";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, objectName)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddPrefix_Empty_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = string.Empty;
            bulkRenamer.SetRenameOperation(addStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddPrefix_ValidPrefix_IsAdded()
        {
            // Arrange
            var name = "Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = "Char_";
            bulkRenamer.SetRenameOperation(addStringOp);

            var expected = "Char_Hero_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddSuffix_Empty_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero_Spawn";
            var bulkRenamer = new BulkRenamer();
            var addStringOp = new AddStringOperation();
            addStringOp.Suffix = string.Empty;
            bulkRenamer.SetRenameOperation(addStringOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddSuffix_ValidPrefix_IsAdded()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var addStringOp = new AddStringOperation();
            addStringOp.Suffix = "_Spawn";
            bulkRenamer.SetRenameOperation(addStringOp);

            var expected = "Char_Hero_Spawn";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNone_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 0;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromFront_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 0;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = "har_Hero";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 1;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = "Char_Her";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteFromFrontAndBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 1;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = "har_Her";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteLongerThanString_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length + 2;
            trimCharactersOp.NumBackDeleteChars = 0;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = string.Empty;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteTooManyFromBothDirections_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length;
            trimCharactersOp.NumBackDeleteChars = name.Length;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = string.Empty;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNegative_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = -1;
            trimCharactersOp.NumBackDeleteChars = -10;
            bulkRenamer.SetRenameOperation(trimCharactersOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_NoFormat_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = string.Empty;
            bulkRenamer.SetRenameOperation(enumerateOp);

            var expected = name;

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_SingleDigitFormat_AddsCount()
        {
            // Arrange
            var bulkRenamer = new BulkRenamer();
            var name = "Char_Hero";
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 0;
            bulkRenamer.SetRenameOperation(enumerateOp);

            var expected = "Char_Hero0";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_CountSeveralTimes_CountsUp()
        {
            // Arrange
            var names = new string[]
            {
                "BlockA",
                "BlockB",
                "BlockC",
                "BlockD",
                "BlockE",
            };
            var bulkRenamer = new BulkRenamer();
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 1;
            bulkRenamer.SetRenameOperation(enumerateOp);

            var expectedNames = new string[]
            {
                "BlockA1",
                "BlockB2",
                "BlockC3",
                "BlockD4",
                "BlockE5",
            };

            // Act
            var results = bulkRenamer.GetRenamedStrings(false, names);

            // Assert
            Assert.AreEqual(
                expectedNames.Length,
                results.Length,
                "Expected Results and results should have the same number of entries but didn't.");
            for (int i = 0; i < results.Length; ++i)
            {
                var expected = expectedNames[i];
                Assert.AreEqual(expected, results[i]);
            }
        }

        [Test]
        public void Enumerating_StartFromNonZero_AddsCorrectCount()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = -1;
            bulkRenamer.SetRenameOperation(enumerateOp);
            var expected = "Char_Hero-1";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_InvalidFormat_IsIgnored()
        {
            // Arrange
            var name = "Char_Hero";
            var bulkRenamer = new BulkRenamer();
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "s";
            enumerateOp.StartingCount = 100;
            bulkRenamer.SetRenameOperation(enumerateOp);
            var expected = "Char_Hero";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AllOperations_ValidOperations_RenamesCorrectly()
        {
            // Arrange
            var bulkRenamer = new BulkRenamer();
            var name = "Char_Hero_Idle";

            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 5;

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "r_H";
            replaceStringOp.ReplacementString = "t_Z";

            var addStringOp = new AddStringOperation();
            addStringOp.Prefix = "a_";
            addStringOp.Suffix = "AA";

            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "D4";
            enumerateOp.StartingCount = 100;

            var listOfOperations = new List<IRenameOperation>();
            listOfOperations.Add(trimCharactersOp);
            listOfOperations.Add(replaceStringOp);
            listOfOperations.Add(addStringOp);
            listOfOperations.Add(enumerateOp);
            bulkRenamer.SetRenameOperations(listOfOperations);

            var expected = "a_hat_ZeroAA0100";

            // Act
            string result = bulkRenamer.GetRenamedStrings(false, name)[0];

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}