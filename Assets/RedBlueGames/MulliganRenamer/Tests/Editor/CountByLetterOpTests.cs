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
    using NUnit.Framework;

    public class CountByLetterOpTests
    {
        [Test]
        public void Rename_NullTarget_Adds()
        {
            // Arrange
            string name = null;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequence(new string[] { "A" });

            var expected = new RenameResult() { new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_EmptySequence_AddsNothing()
        {
            // Arrange
            string name = "Blah";
            var countByLetterOp = new CountByLetterOperation();

            var expected = new RenameResult() { new Diff("Blah", DiffOperation.Equal) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAddFirstToEmpty_AddsA()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAddFirstToSomething_AddsA()
        {
            // Arrange
            string name = "Something"; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() {
                new Diff("Something", DiffOperation.Equal),
                new Diff("A", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAddFirstToSomethingPrefixed_PrependsA()
        {
            // Arrange
            string name = "Something"; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);
            countByLetterOp.Prepend = true;

            var expected = new RenameResult() {
                new Diff("A", DiffOperation.Insertion),
                new Diff("Something", DiffOperation.Equal) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAdd26thToEmpty_AddsZ()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("Z", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 25);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAdd27thToEmpty_AddsAA()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("AA", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 26);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAdd28thToEmpty_AddsAB()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("AB", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 27);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAdd27x26thToEmpty_AddsZZ()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("ZZ", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, (27 * 26) - 1);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_UppercaseAdd27x26AndOnethToEmpty_AddsAAA()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            var expected = new RenameResult() { new Diff("AAA", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, (27 * 26));

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_Uppercase_AddsAllLetters()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);

            // Act / Assert
            for (int i = 0; i < 25; ++i)
            {
                var result = countByLetterOp.Rename(name, i);
                var letter = (char)('A' + i);
                var expected = new RenameResult() { new Diff(letter.ToString(), DiffOperation.Insertion) };
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void Rename_Lowercase_AddsAllLetters()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.LowercaseAlphabet);

            // Act / Assert
            for (int i = 0; i < 25; ++i)
            {
                var result = countByLetterOp.Rename(name, i);
                var letter = (char)('a' + i);
                var expected = new RenameResult() { new Diff(letter.ToString(), DiffOperation.Insertion) };
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void Rename_StartingFromB_StartsWithB()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);
            countByLetterOp.StartingCount = 1;

            var expected = new RenameResult() { new Diff("B", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_StartingFromZ_NextIsAA()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);
            countByLetterOp.StartingCount = 25;

            var expected = new RenameResult() { new Diff("AA", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 1);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_StartingFromBB_NextIsBC()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequencePreset(CountByLetterOperation.StringPreset.UppercaseAlphabet);
            countByLetterOp.StartingCount = 53;

            var expected = new RenameResult() { new Diff("BC", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 1);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_DoNotCarryOver_LoopsToNext()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequence(new string[] { "X", "Y", "Z" });
            countByLetterOp.DoNotCarryOver = true;

            var expected = new RenameResult() { new Diff("X", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 3);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Rename_Increment_CountsByIncrement()
        {
            // Arrange
            string name = string.Empty; ;
            var countByLetterOp = new CountByLetterOperation();
            countByLetterOp.SetCountSequence(new string[] { "X", "Y", "Z" });
            countByLetterOp.Increment = 2;

            var expected = new RenameResult() { new Diff("XY", DiffOperation.Insertion) };

            // Act
            var result = countByLetterOp.Rename(name, 2);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}