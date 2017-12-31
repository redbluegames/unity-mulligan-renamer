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

            var expected = new RenameResult() { new Diff("0", DiffOperation.Insertion) };

            // Act
            var result = enumerateOp.Rename(name, 0);

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

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = enumerateOp.Rename(name, 0);

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

            var expected = new RenameResult()
            {
                new Diff(name, DiffOperation.Equal),
                new Diff("0", DiffOperation.Insertion)
            };

            // Act
            var result = enumerateOp.Rename(name, 0);

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

            var expectedRenameResults = new RenameResult[]
            {
                new RenameResult() { new Diff("BlockA", DiffOperation.Equal), new Diff("1", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockB", DiffOperation.Equal), new Diff("2", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockC", DiffOperation.Equal), new Diff("3", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockD", DiffOperation.Equal), new Diff("4", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockE", DiffOperation.Equal), new Diff("5", DiffOperation.Insertion) },
            };

            // Act
            var results = new List<RenameResult>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                results.Add(enumerateOp.Rename(names[i], i));
            }

            // Assert
            Assert.AreEqual(
                expectedRenameResults.Length,
                results.Count,
                "Expected Results and results should have the same number of entries but didn't.");
            for (int i = 0; i < results.Count; ++i)
            {
                var expected = expectedRenameResults[i];
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


            var expected = new RenameResult()
            {
                new Diff("Char_Hero", DiffOperation.Equal),
                new Diff("-1", DiffOperation.Insertion)
            };

            // Act
            var result = enumerateOp.Rename(name, 0);

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

            var expected = new RenameResult() { new Diff(name, DiffOperation.Equal) };

            // Act
            var result = enumerateOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RenameCountIncrement_ByTwo_CountsUpByTwo()
        {
            // Arrange
            var names = new string[]
            {
                "BlockA",
                "BlockB",
                "BlockC",
            };

            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 1;
            enumerateOp.Increment = 2;

            var expectedRenameResults = new RenameResult[]
            {
                new RenameResult() { new Diff("BlockA", DiffOperation.Equal), new Diff("1", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockB", DiffOperation.Equal), new Diff("3", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockC", DiffOperation.Equal), new Diff("5", DiffOperation.Insertion) },
            };

            // Act
            var results = new List<RenameResult>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                results.Add(enumerateOp.Rename(names[i], i));
            }

            // Assert
            Assert.AreEqual(
                expectedRenameResults.Length,
                results.Count,
                "Expected Results and results should have the same number of entries but didn't.");
            for (int i = 0; i < results.Count; ++i)
            {
                var expected = expectedRenameResults[i];
                Assert.AreEqual(expected, results[i]);
            }
        }

        [Test]
        public void RenameCountIncrement_ByNegativeOne_CountsDown()
        {
            // Arrange
            var names = new string[]
            {
                "BlockA",
                "BlockB",
                "BlockC",
            };

            var enumerateOp = new EnumerateOperation();
            enumerateOp.CountFormat = "0";
            enumerateOp.StartingCount = 1;
            enumerateOp.Increment = -1;

            var expectedRenameResults = new RenameResult[]
            {
                new RenameResult() { new Diff("BlockA", DiffOperation.Equal), new Diff("1", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockB", DiffOperation.Equal), new Diff("0", DiffOperation.Insertion) },
                new RenameResult() { new Diff("BlockC", DiffOperation.Equal), new Diff("-1", DiffOperation.Insertion) },
            };

            // Act
            var results = new List<RenameResult>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                results.Add(enumerateOp.Rename(names[i], i));
            }

            // Assert
            Assert.AreEqual(
                expectedRenameResults.Length,
                results.Count,
                "Expected Results and results should have the same number of entries but didn't.");
            for (int i = 0; i < results.Count; ++i)
            {
                var expected = expectedRenameResults[i];
                Assert.AreEqual(expected, results[i]);
            }
        }
    }
}