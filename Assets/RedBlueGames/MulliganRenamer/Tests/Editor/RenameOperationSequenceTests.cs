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

    public class RenameOperationSequenceTests
    {
        [Test]
        public void GetNewName_AllOperations_RenamesCorrectly()
        {
            // Arrange
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
            enumerateOp.SetCountFormat("D4");
            enumerateOp.StartingCount = 100;

            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            operationSequence.Add(trimCharactersOp);
            operationSequence.Add(replaceStringOp);
            operationSequence.Add(addStringOp);
            operationSequence.Add(enumerateOp);

            var expected = "a_hat_ZeroAA0100";

            // Act
            string result = operationSequence.GetResultingName(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetNewName_ValidOperations_AreAppliedInOrder()
        {
            // Arrange
            var name = "Char_Hero_Idle";

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "ar_He";
            replaceStringOp.ReplacementString = "baboon";

            var trimCharactersOp = new TrimCharactersOperation();
            trimCharactersOp.NumFrontDeleteChars = 4;

            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            operationSequence.Add(replaceStringOp);
            operationSequence.Add(trimCharactersOp);

            var operationSequenceReversed = new RenameOperationSequence<IRenameOperation>();
            operationSequenceReversed.Add(trimCharactersOp);
            operationSequenceReversed.Add(replaceStringOp);

            var expected = "boonro_Idle";
            var expectedReversed = "_Hero_Idle";

            // Act
            string result = operationSequence.GetResultingName(name, 0);
            string resultReversed = operationSequenceReversed.GetResultingName(name, 0);

            // Assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(expectedReversed, resultReversed);
        }

        [Test]
        public void SerializeToString_OneOperation_Serialize()
        {
            var dummyOperation = new DummyOperation();
            dummyOperation.Value = "Test";

            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            operationSequence.Add(dummyOperation);

            string expectedSerializedString =
                "[RedBlueGames.MulliganRenamer.RenameOperationSequenceTests+DummyOperation]" +
                "{\"value\":\"Test\"}";

            Assert.AreEqual(expectedSerializedString, operationSequence.ToSerializableString());
        }

        [Test]
        public void SerializeToString_TwoOperations_Serialize()
        {
            var dummyOperation1 = new DummyOperation();
            dummyOperation1.Value = "First value";

            var dummyOperation2 = new DummyOperation();
            dummyOperation2.Value = "The next value";

            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            operationSequence.Add(dummyOperation1);
            operationSequence.Add(dummyOperation2);

            string expectedSerializedString =
                "[RedBlueGames.MulliganRenamer.RenameOperationSequenceTests+DummyOperation]" +
                "{\"value\":\"First value\"}\n" +
                "[RedBlueGames.MulliganRenamer.RenameOperationSequenceTests+DummyOperation]" +
                "{\"value\":\"The next value\"}";

            Assert.AreEqual(expectedSerializedString, operationSequence.ToSerializableString());
        }

        [Test]
        public void DeserializeFromString_ValidOperations_Deserializes()
        {
            // Arrange
            string serializedString =
                "[" + typeof(DummyOperation).AssemblyQualifiedName + "]" +
                "{\"value\":\"Serialized value\"}";

            var dummyOperation = new DummyOperation();
            dummyOperation.Value = "Serialized value";
            var expectedSequence = new RenameOperationSequence<IRenameOperation>();
            expectedSequence.Add(dummyOperation);

            // Act
            var deserializedSequence = RenameOperationSequence<IRenameOperation>.FromString(serializedString);

            // Assert
            CollectionAssert.AreEqual(expectedSequence, deserializedSequence);
        }

        [System.Serializable]
        private class DummyOperation : IRenameOperation
        {
            [SerializeField]
            private string value;

            public string Value
            {
                get
                {
                    return this.value;
                }

                set
                {
                    this.value = value;
                }
            }

            public IRenameOperation Clone()
            {
                return new DummyOperation() { value = this.value };
            }

            public bool HasErrors()
            {
                return false;
            }

            public RenameResult Rename(string input, int relativeCount)
            {
                var result = new RenameResult();
                result.Add(new Diff(input, DiffOperation.Equal));
                return result;
            }

            public override bool Equals(object obj)
            {
                var item = obj as DummyOperation;

                if (item == null)
                {
                    return false;
                }

                return item.Value.Equals(this.value);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}