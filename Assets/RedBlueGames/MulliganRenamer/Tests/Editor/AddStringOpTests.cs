namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class AddStringOpTests
    {
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
    }
}