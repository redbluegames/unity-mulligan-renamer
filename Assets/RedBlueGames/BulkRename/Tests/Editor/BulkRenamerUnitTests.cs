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
            replaceStringOp.RegexSearchString = string.Empty;

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
            replaceStringOp.RegexSearchString = "Hero";
            replaceStringOp.RegexReplacementString = "A";

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
            replaceStringOp.RegexSearchString = "o";

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
            replaceStringOp.RegexSearchString = "Heroine";

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
            replaceStringOp.RegexSearchString = "Hero";
            replaceStringOp.RegexReplacementString = "Link";

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
            replaceStringOp.RegexSearchString = "ZelDa";
            replaceStringOp.RegexReplacementString = "blah";

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
            replaceStringOp.RegexSearchString = "zelda";
            replaceStringOp.RegexReplacementString = "blah";

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
            replaceStringOp.RegexSearchString = "ZeldA";
            replaceStringOp.RegexReplacementString = "blah";

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
            replaceStringOp.RegexSearchString = "[a-zA-Z]*_";
            replaceStringOp.RegexReplacementString = "Yep";

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
            replaceStringOp.RegexSearchString = "\\.";
            replaceStringOp.RegexReplacementString = "_";

            var expected = "Char_Hero_Woot";

            // Act
            string result = replaceStringOp.Rename(name, 0);

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

            var expected = name;

            // Act
            string result = addStringOp.Rename(name, 0);

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

            var expected = "Char_Hero_Spawn";

            // Act
            string result = addStringOp.Rename(name, 0);

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

            var expected = name;

            // Act
            string result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddSuffix_ValidPrefix_IsAdded()
        {
            // Arrange
            var name = "Char_Hero";
            var addStringOp = new AddStringOperation();
            addStringOp.Suffix = "_Spawn";

            var expected = "Char_Hero_Spawn";

            // Act
            string result = addStringOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNone_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = name;

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromFront_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = "har_Hero";

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteOneFromBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 0;
            trimCharactersOp.NumBackDeleteChars = 1;

            var expected = "Char_Her";

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteFromFrontAndBack_IsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 1;
            trimCharactersOp.NumBackDeleteChars = 1;

            var expected = "har_Her";

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteLongerThanString_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length + 2;
            trimCharactersOp.NumBackDeleteChars = 0;

            var expected = string.Empty;

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteTooManyFromBothDirections_EntireStringIsDeleted()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = name.Length;
            trimCharactersOp.NumBackDeleteChars = name.Length;

            var expected = string.Empty;

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Trimming_DeleteNegative_IsUnchanged()
        {
            // Arrange
            var name = "Char_Hero";
            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = -1;
            trimCharactersOp.NumBackDeleteChars = -10;

            var expected = name;

            // Act
            string result = trimCharactersOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_NoFormat_DoesNothing()
        {
            // Arrange
            var name = "Char_Hero";
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = string.Empty;

            var expected = name;

            // Act
            string result = enumerateOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_SingleDigitFormat_AddsCount()
        {
            // Arrange
            var name = "Char_Hero";
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 0;

            var expected = "Char_Hero0";

            // Act
            string result = enumerateOp.Rename(name, 0);

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
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 1;

            var expectedNames = new string[]
            {
                "BlockA1",
                "BlockB2",
                "BlockC3",
                "BlockD4",
                "BlockE5",
            };

            // Act
            var results = new List<string>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                results.Add(enumerateOp.Rename(names[i], i));
            }

            // Assert
            Assert.AreEqual(
                expectedNames.Length,
                results.Count,
                "Expected Results and results should have the same number of entries but didn't.");
            for (int i = 0; i < results.Count; ++i)
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
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = -1;
            var expected = "Char_Hero-1";

            // Act
            string result = enumerateOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Enumerating_InvalidFormat_IsIgnored()
        {
            // Arrange
            var name = "Char_Hero";
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "s";
            enumerateOp.StartingCount = 100;
            var expected = "Char_Hero";

            // Act
            string result = enumerateOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void BulkRenamer_AllOperations_RenamesCorrectly()
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
            string result = bulkRenamer.GetRenamePreviews(name)[0].NewName;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void BulkRenamer_OrderSensitiveOps_RespectsOrder()
        {
            // Arrange
            var name = "Char_Hero_Idle";

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "ar_He";
            replaceStringOp.ReplacementString = "baboon";

            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 4;

            var listOfOperations = new List<IRenameOperation>();
            listOfOperations.Add(replaceStringOp);
            listOfOperations.Add(trimCharactersOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.SetRenameOperations(listOfOperations);

            var renamerReversed = new BulkRenamer();
            var operationsReversed = new List<IRenameOperation>();
            operationsReversed.Add(trimCharactersOp);
            operationsReversed.Add(replaceStringOp);
            renamerReversed.SetRenameOperations(operationsReversed);

            var expected = "boonro_Idle";
            var expectedReversed = "_Hero_Idle";

            // Act
            string result = bulkRenamer.GetRenamePreviews(name)[0].NewName;
            string resultReversed = renamerReversed.GetRenamePreviews(name)[0].NewName;

            // Assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(expectedReversed, resultReversed);
        }
    }
}