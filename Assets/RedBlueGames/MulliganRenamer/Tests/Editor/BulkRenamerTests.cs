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

    using Object = UnityEngine.Object;

    public class BulkRenamerTests
    {
        private static readonly string TestFixturesFolderName = "TestFixtures";
        private static readonly string TestFixturesPath = 
            string.Concat("Assets/", TestFixturesFolderName);
        private static readonly string TestFixturesDirectory =
            string.Concat(TestFixturesPath, "/");

        private List<Object> singleAsset;
        private List<Object> multipleAssets;
        private List<Object> enumeratedObjects;

        private List<Object> gameObjects;
        private Texture2D textureWithSprites;

        [OneTimeSetUp]
        public void Setup()
        {
            AssetDatabase.CreateFolder("Assets", TestFixturesFolderName);

            this.SetupSingleAssetTest();
            this.SetupMultipleAssetsTest();
            this.SetupGameObjects();
            this.SetupEnumeratedObject();
            this.SetupSpriteSheet();
        }

        private void SetupSingleAssetTest()
        {
            var gameObjectAsset = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Original.prefab"),
                                      new GameObject("Original"));
            this.singleAsset = new List<Object>();
            this.singleAsset.Add(gameObjectAsset);
        }

        private void SetupMultipleAssetsTest()
        {
            var multipleObject0 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Asset0.prefab"),
                                      new GameObject("Asset0"));
            var multipleObject1 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Asset1.prefab"),
                                      new GameObject("Asset1"));

            this.multipleAssets = new List<Object>();
            this.multipleAssets.Add(multipleObject0);
            this.multipleAssets.Add(multipleObject1);
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

            this.enumeratedObjects = new List<Object>();
            this.enumeratedObjects.Add(enumeratedObject0);
            this.enumeratedObjects.Add(enumeratedObject1);
            this.enumeratedObjects.Add(enumeratedObject2);
        }

        private void SetupGameObjects()
        {
            var gameObject0 = new GameObject("GameObject0");
            var gameObject1 = new GameObject("GameObject1");

            this.gameObjects = new List<Object>();
            this.gameObjects.Add(gameObject0);
            this.gameObjects.Add(gameObject1);
        }

        private void SetupSpriteSheet()
        {
            var texture = new Texture2D(64, 64, TextureFormat.ARGB32, false, true);
            for (int x = 0; x < 64; ++x)
            {
                for (int y = 0; y < 64; ++y)
                {
                    texture.SetPixel(x, y, Color.cyan);
                }
            }

            texture.Apply();

            // Need to save the texture as an Asset and store a reference to the Asset
            var path = string.Concat(TestFixturesDirectory, "Texture.png");
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            this.textureWithSprites = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            var spriteMetaData = new SpriteMetaData[4];
            spriteMetaData[0].rect = new Rect(0, 0, 32, 32);
            spriteMetaData[0].name = "Texture_Sprite0";
            spriteMetaData[1].rect = new Rect(32, 0, 32, 32);
            spriteMetaData[1].name = "Texture_Sprite1";
            spriteMetaData[2].rect = new Rect(0, 32, 32, 32);
            spriteMetaData[2].name = "Texture_Sprite2";
            spriteMetaData[3].rect = new Rect(32, 32, 32, 32);
            spriteMetaData[3].name = "Texture_Sprite3";
            importer.spritesheet = spriteMetaData;
            importer.SaveAndReimport();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFixturesPath);
        }

        [Test]
        public void RenameObjects_SingleAsset_Renames()
        {
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewName";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.singleAsset, renameSequence, true);

            Assert.AreEqual("NewName", this.singleAsset[0].name);
        }

        [Test]
        public void RenameObjects_MultipleAssets_Renames()
        {
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Asset";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceStringOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.multipleAssets, renameSequence, true);

            var expectedNames = new List<string>
            {
                "Thingy0",
                "Thingy1"
            };

            var resultingNames = new List<string>();
            foreach (var obj in this.multipleAssets)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_MultipleGameObjects_Renames()
        {
            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Object";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceStringOp);

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(this.gameObjects, renameSequence, true);

            var expectedNames = new List<string>
            {
                "GameThingy0",
                "GameThingy1"
            };

            var resultingNames = new List<string>();
            foreach (var obj in this.gameObjects)
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

        [Test]
        public void RenameObjects_Spritesheet_Renames()
        {
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewSprite";

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);
            renameSequence.Add(enumerateOp);

            var path = AssetDatabase.GetAssetPath(this.textureWithSprites);
            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            var allSprites = new List<Object>();
            foreach (var asset in allAssetsAtPath)
            {
                if (asset is Sprite)
                {
                    allSprites.Add(asset);
                }
            }

            var bulkRenamer = new BulkRenamer();
            bulkRenamer.RenameObjects(allSprites, renameSequence, true);

            var expectedNames = new List<string>
            {
                "NewSprite1",
                "NewSprite2",
                "NewSprite3",
                "NewSprite4"
            };

            var resultingNames = new List<string>();
            foreach (var sprite in allSprites)
            {
                resultingNames.Add(sprite.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }
    }
}
