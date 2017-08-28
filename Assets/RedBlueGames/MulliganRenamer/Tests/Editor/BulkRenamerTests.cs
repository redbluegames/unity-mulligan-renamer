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

    public class BulkRenamerTests
    {
        [Test]
        public void GetRenamePreviews_AllOperations_RenamesCorrectly()
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
        public void GetRenamePreviewsOrder_ValidOperations_AreAppliedInOrder()
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