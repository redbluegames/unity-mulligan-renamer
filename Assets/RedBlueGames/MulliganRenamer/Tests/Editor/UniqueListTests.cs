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
    using UnityEngine;
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
        public void AddRange_Duplicates_IgnoresDuplicate()
        {
            // Act
            var list = new UniqueList<string>();

            list.AddRange(new List<string>() { "Test", "Test" });

            // Assert
            CollectionAssert.AreEquivalent(new[] { "Test", }, list);
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