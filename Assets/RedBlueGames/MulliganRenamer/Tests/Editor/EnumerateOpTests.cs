namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class EnumerateOpTests
    {
        [Test]
        public void Rename_NullTarget_AddsCount()
        {
            // Arrange
            string name = null;
            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 0;

            var expected = "0";

            // Act
            string result = enumerateOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameFormat_NoFormat_DoesNothing()
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
        public void RenameFormat_SingleDigitFormat_AddsCount()
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
        public void RenameCount_CountSeveralItems_CountsUp()
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
        public void RenameStartingCount_StartFromNonZero_AddsCorrectCount()
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
        public void RenameStartingCount_InvalidFormat_IsIgnored()
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
    }
}