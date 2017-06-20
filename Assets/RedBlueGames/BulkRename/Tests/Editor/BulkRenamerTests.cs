namespace RedBlueGames.BulkRename.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class BulkRenamerTests
    {
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