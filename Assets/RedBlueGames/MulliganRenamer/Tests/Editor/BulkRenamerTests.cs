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

        [SetUp]
        public void Setup()
        {
            AssetDatabase.CreateFolder("Assets", TestFixturesFolderName);
            AssetDatabase.CreateFolder(TestFixturesPath, "SubDirectory");
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFixturesPath);
        }

        [Test]
        public void RenameObjects_SingleAsset_Renames()
        {
            var gameObjectAsset = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Original.prefab"),
                                      new GameObject("Original"));
            var singleAsset = new List<Object>() { gameObjectAsset };

            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewName";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(singleAsset, true);

            Assert.AreEqual("NewName", singleAsset[0].name);
        }

        [Test]
        public void RenameObjects_MultipleAssets_Renames()
        {
            // Arrange
            var multipleObject0 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Asset0.prefab"),
                                      new GameObject("Asset0"));
            var multipleObject1 = PrefabUtility.CreatePrefab(
                                      string.Concat(TestFixturesDirectory, "Asset1.prefab"),
                                      new GameObject("Asset1"));
            var multipleAssets = new List<Object>()
            {
                multipleObject0,
                multipleObject1
            };

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Asset";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceStringOp);

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(multipleAssets, true);

            var expectedNames = new List<string>
            {
                "Thingy0",
                "Thingy1"
            };

            var resultingNames = new List<string>();
            foreach (var obj in multipleAssets)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_MultipleGameObjects_Renames()
        {

            var gameObject0 = new GameObject("GameObject0");
            var gameObject1 = new GameObject("GameObject1");

            var gameObjects = new List<Object>()
            {
                gameObject0,
                gameObject1
            };

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Object";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceStringOp);

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(gameObjects, true);

            var expectedNames = new List<string>
            {
                "GameThingy0",
                "GameThingy1"
            };

            var resultingNames = new List<string>();
            foreach (var obj in gameObjects)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_Spritesheet_Renames()
        {
            // Arrange
            var textureWithSprites = this.SetupSpriteSheet(2, "Texture_Sprite");
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewSprite";

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);
            renameSequence.Add(enumerateOp);

            var path = AssetDatabase.GetAssetPath(textureWithSprites);
            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            var allSprites = new List<Object>();
            foreach (var asset in allAssetsAtPath)
            {
                if (asset is Sprite)
                {
                    allSprites.Add(asset);
                }
            }

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(allSprites, true);

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

        [Test]
        public void RenameObjects_EnumeratedObjectsWithDependentChanges_Renames()
        {
            // Arrange
            var enumeratedObject0 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject0.prefab"),
                                        new GameObject("EnumeratedObject0"));
            var enumeratedObject1 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject1.prefab"),
                                        new GameObject("EnumeratedObject1"));
            var enumeratedObject2 = PrefabUtility.CreatePrefab(
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject2.prefab"),
                                        new GameObject("EnumeratedObject2"));

            var enumeratedObjects = new List<Object>()
            {
                enumeratedObject0,
                enumeratedObject1,
                enumeratedObject2,
            };

            var removeCharactersOp = new RemoveCharactersOperation();
            var removeCharacterOptions = RemoveCharactersOperation.Numbers;
            removeCharactersOp.Options = removeCharacterOptions;

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(removeCharactersOp);
            renameSequence.Add(enumerateOp);

            // Act
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(enumeratedObjects, true);

            // Assert
            // Build two lists to compare against because Assert displays their differences nicely in its output.
            var expectedNames = new List<string>
            {
                "EnumeratedObject1",
                "EnumeratedObject2",
                "EnumeratedObject3"
            };

            var resultingNames = new List<string>();
            foreach (var obj in enumeratedObjects)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_ChangeObjectToExistingObjectName_DoesNothing()
        {
            // Arrange
            var conflictingObject0 = PrefabUtility.CreatePrefab(
                string.Concat(TestFixturesDirectory, "ConflictingObject0.prefab"),
                new GameObject("ConflictingObject0"));
            var existingObject = PrefabUtility.CreatePrefab(
                string.Concat(TestFixturesDirectory, "ExistingObject.prefab"),
                new GameObject("ExistingObject"));

            var conflictingObjectsWithoutAllNamesChanging = new List<Object>();
            conflictingObjectsWithoutAllNamesChanging.Add(conflictingObject0);
            conflictingObjectsWithoutAllNamesChanging.Add(existingObject);

            var replaceFirstNameOp = new ReplaceStringOperation();
            replaceFirstNameOp.SearchString = "ConflictingObject0";
            replaceFirstNameOp.ReplacementString = "ExistingObject";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceFirstNameOp);

            // Act and Assert
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(conflictingObjectsWithoutAllNamesChanging);
            var expectedName = "ConflictingObject0";
            Assert.AreEqual(expectedName, conflictingObject0.name);
        }

        [Test]
        public void RenameObjects_RenameObjectToExistingObjectNameButAtDifferentPath_Succeeds()
        {
            // Arrange
            var conflictingObject0 = PrefabUtility.CreatePrefab(
                string.Concat(TestFixturesDirectory, "ConflictingObject0.prefab"),
                new GameObject("ConflictingObject0"));
            var existingObject = PrefabUtility.CreatePrefab(
                string.Concat(TestFixturesDirectory, "SubDirectory/ExistingObject.prefab"),
                new GameObject("ExistingObject"));

            var replaceFirstNameOp = new ReplaceStringOperation();
            replaceFirstNameOp.SearchString = "ConflictingObject0";
            replaceFirstNameOp.ReplacementString = "ExistingObject";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceFirstNameOp);

            var expectedNames = new List<string>
            {
                "ExistingObject",
                "ExistingObject"
            };

            var objectsToRename = new List<UnityEngine.Object>()
            {
                conflictingObject0,
                existingObject,
            };

            // Act and Assert
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(objectsToRename);

            var resultingNames = new List<string>();
            foreach (var obj in objectsToRename)
            {
                resultingNames.Add(obj.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_SpritesheetWithTargetNameAsSubstringInMultipleSprites_Renames()
        {
            // Arrange
            var textureWithSprites = this.SetupSpriteSheet(4, "Texture_Sprite");
            var replaceNameOp = new ReplaceStringOperation();
            replaceNameOp.SearchString = "Texture_Sprite1";
            replaceNameOp.ReplacementString = "CoolSprite";

            var renameSequence = new RenameOperationSequence<RenameOperation>();
            renameSequence.Add(replaceNameOp);

            var path = AssetDatabase.GetAssetPath(textureWithSprites);
            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            var allSprites = new List<Object>();
            foreach (var asset in allAssetsAtPath)
            {
                if (asset is Sprite)
                {
                    allSprites.Add(asset);
                }
            }

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(new List<Object>() { allSprites[0] }, true);

            var expectedNames = new List<string>
            {
                "CoolSprite",
                "Texture_Sprite2",
                "Texture_Sprite3",
                "Texture_Sprite4",
                "Texture_Sprite5",
                "Texture_Sprite6",
                "Texture_Sprite7",
                "Texture_Sprite8",
                "Texture_Sprite9",
                "Texture_Sprite10",
                "Texture_Sprite11",
                "Texture_Sprite12",
                "Texture_Sprite13",
                "Texture_Sprite14",
                "Texture_Sprite15",
                "Texture_Sprite16",
            };

            var resultingNames = new List<string>();
            foreach (var sprite in allSprites)
            {
                resultingNames.Add(sprite.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        private Texture2D SetupSpriteSheet(int cellsPerSide, string namePrefix)
        {
            var cellSize = 32;
            var texture = new Texture2D(
                cellSize * cellsPerSide,
                cellSize * cellsPerSide,
                TextureFormat.ARGB32,
                false,
                true);
            var size = Vector2.one * cellSize * cellsPerSide;
            for (int x = 0; x < size.x; ++x)
            {
                for (int y = 0; y < size.y; ++y)
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
            var textureWithSprites = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            var spriteMetaData = new SpriteMetaData[cellsPerSide * cellsPerSide];
            for (int i = 0; i < cellsPerSide; ++i)
            {
                for (int j = 0; j < cellsPerSide; ++j)
                {
                    var cellIndex = i * cellsPerSide + j;
                    var x = i * cellSize;
                    var y = j * cellSize;
                    spriteMetaData[cellIndex].rect = new Rect(x, y, cellSize, cellSize);
                    var name = string.Concat(namePrefix, (cellIndex + 1).ToString());
                    spriteMetaData[cellIndex].name = name;
                }
            }

            importer.spritesheet = spriteMetaData;
            importer.SaveAndReimport();

            return textureWithSprites;
        }
    }
}
