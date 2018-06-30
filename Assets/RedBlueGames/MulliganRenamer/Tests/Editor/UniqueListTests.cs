namespace RedBlueGames.MulliganRenamer
{
    using UnityEngine;
    using UnityEngine.TestTools;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class UniqueListTests
    {
        [Test]
        public void Add_IntoEmpty_Adds()
        {
            // Act
            var list = new UniqueList<string>();

            // Arrange
            list.Add("Test");

            // Assert
            CollectionAssert.AreEquivalent(new[] { "Test" }, list);
        }

        [Test]
        public void Add_MultipleUniques_Adds()
        {
            // Act
            var list = new UniqueList<string>();

            // Arrange
            list.Add("TestA");
            list.Add("TestB");

            // Assert
            CollectionAssert.AreEquivalent(new[] { "TestA", "TestB" }, list);
        }

        [Test]
        public void Add_Duplicates_Throws()
        {
            // Act
            var list = new UniqueList<string>();

            // Arrange
            list.Add("Test");

            // Assert
            Assert.Throws<System.InvalidOperationException>(() => list.Add("Test"));
        }

        [Test]
        public void AddRange_ValidElements_Adds()
        {
            // Act
            var list = new UniqueList<string>();

            // Arrange
            list.AddRange(new List<string>() { "TestA", "TestB" });

            // Assert
            CollectionAssert.AreEquivalent(new[] { "TestA", "TestB" }, list);
        }

        [Test]
        public void AddRange_Duplicates_Throws()
        {
            // Act
            var list = new UniqueList<string>();

            // Assert
            Assert.Throws<System.InvalidOperationException>(() => list.AddRange(new List<string>() { "Test", "Test" }));
        }

        [Test]
        public void Clear_Empty_Clears()
        {
            // Act
            var list = new UniqueList<string>();

            // Arrange
            list.Clear();

            // Assert
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void Clear_NonEmpty_Clears()
        {
            // Act
            var list = new UniqueList<string>();
            list.Add("Test");

            // Arrange
            list.Clear();

            // Assert
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void Clear_NonEmptyAdd_Adds()
        {
            // Act
            var entry = "Test";
            var list = new UniqueList<string>();
            list.Add(entry);

            // Arrange
            list.Clear();
            list.Add(entry);

            // Assert
            CollectionAssert.AreEquivalent(new[] { entry }, list);
        }

        [Test]
        public void Remove_Element_Removes()
        {
            // Act
            var list = new UniqueList<string>();
            list.Add("Test");

            // Arrange
            list.Remove("Test");

            // Assert
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void Remove_ElementThenAdd_Adds()
        {
            // Act
            var entry = "Test";
            var list = new UniqueList<string>();
            list.Add(entry);

            // Arrange
            list.Remove(entry);
            list.Add(entry);

            // Assert
            CollectionAssert.AreEquivalent(new[] { entry }, list);
        }

        [Test]
        public void RemoveAt_ValidIndex_Removes()
        {
            // Act
            var list = new UniqueList<string>();
            list.Add("Test");

            // Arrange
            list.RemoveAt(0);

            // Assert
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void RemoveAt_ValidIndexThenAdd_Adds()
        {
            // Act
            var entry = "Test";
            var list = new UniqueList<string>();
            list.Add(entry);

            // Arrange
            list.RemoveAt(0);
            list.Add(entry);

            // Assert
            CollectionAssert.AreEquivalent(new[] { entry }, list);
        }
    }
}