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
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using NUnit.Framework;

    public class BulkRenamerTests
    {
        private static readonly string TestFixturesFolderName = "TestFixtures";
        private static readonly string TestFixturesPath = 
            string.Concat("Assets/", TestFixturesFolderName);
        private static readonly string TestFixturesDirectory =
            string.Concat(TestFixturesPath, "/");

        private List<UnityEngine.Object> singleObject;
        private List<UnityEngine.Object> multipleObjects;
        private List<UnityEngine.Object> enumeratedObjects;

        [OneTimeSetUp]
        public void Setup()
        {
            AssetDatabase.CreateFolder("Assets", TestFixturesFolderName);

            this.SetupSingleObjectTest();
            this.SetupMultipleObjectTest();
            this.SetupEnumeratedObject();
        }

        private void SetupSingleObjectTest()
        {
            var gameObjectAsset = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Original.prefab"),
                                      new GameObject("Original"));
            this.singleObject = new List<UnityEngine.Object>();
            this.singleObject.Add(gameObjectAsset);
        }

        private void SetupMultipleObjectTest()
        {
            var multipleObject0 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "GameObject0.prefab"),
                                      new GameObject("GameObject0"));
            var multipleObject1 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "GameObject1.prefab"),
                                      new GameObject("GameObject1"));

            this.multipleObjects = new List<UnityEngine.Object>();
            this.multipleObjects.Add(multipleObject0);
            this.multipleObjects.Add(multipleObject1);
        }

        private void SetupEnumeratedObject()
        {
            var enumeratedObject0 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject0.prefab"),
                                        new GameObject("EnumeratedObject0"));
            var enumeratedObject1 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject1.prefab"),
                                        new GameObject("EnumeratedObject1"));
            var enumeratedObject2 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject2.prefab"),
                                        new GameObject("EnumeratedObject2"));

            this.enumeratedObjects = new List<UnityEngine.Object>();
            this.enumeratedObjects.Add(enumeratedObject0);
            this.enumeratedObjects.Add(enumeratedObject1);
            this.enumeratedObjects.Add(enumeratedObject2);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFixturesPath);
        }

        [Test]
        public void RenameObjects_SingleObject_Renames()
        {
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewName";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.singleObject, renameSequence, true);

            Assert.AreEqual("NewName", this.singleObject[0].name);
        }

        [Test]
        public void RenameObjects_MultipleObjects_Renames()
        {
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Object";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceStringOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.multipleObjects, renameSequence, true);

            var expectedNames = new List<string>
            {
                "GameThingy0",
                "GameThingy1"
            };

            var resultingNames = new List<string>();
            foreach (var obj in this.multipleObjects)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_EnumeratedObjectsWithDependentChanges_Renames()
        {
            // Arrange
            var removeCharactersOp = new RemoveCharactersOperation();
            var removeCharacterOptions = RemoveCharactersOperation.Numbers;
            removeCharactersOp.Options = removeCharacterOptions;

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(removeCharactersOp);
            renameSequence.Add(enumerateOp);

            // Act
            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.enumeratedObjects, renameSequence, true);

            // Assert
            // Build two lists to compare against because Assert displays their differences nicely in its output.
            var expectedNames = new List<string>
            {
                "EnumeratedObject1",
                "EnumeratedObject2",
                "EnumeratedObject3"
            };

            var resultingNames = new List<string>();
            foreach (var obj in this.enumeratedObjects)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }
    }
}
