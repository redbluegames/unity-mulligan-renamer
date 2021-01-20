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
    using NUnit.Framework;

    using Object = UnityEngine.Object;

    public class BulkRenamerTests
    {
        private static readonly string TestFixturesFolderName = "TestFixtures";
        private static readonly string TestFixturesPath =
            string.Concat("Assets/", TestFixturesFolderName);
        private static readonly string TestFixturesDirectory =
            string.Concat(TestFixturesPath, "/");

        private GameObject CreatePrefabFromGameObject(GameObject originalObject, string newName)
        {
#if UNITY_2018_3_OR_NEWER
            return PrefabUtility.SaveAsPrefabAsset(originalObject, newName);                                      
#else
            return PrefabUtility.CreatePrefab(string.Concat(TestFixturesDirectory, "Original.prefab"), originalObject);
#endif
        }

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
            var gameObjectAsset = this.CreatePrefabFromGameObject(
                                    new GameObject("Original"),
                                    string.Concat(TestFixturesDirectory, "Original.prefab"));
            var singleAsset = new List<Object>() { gameObjectAsset };

            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewName";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(replaceNameOp);

            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(singleAsset, true);

            Assert.AreEqual("NewName", singleAsset[0].name);
        }

        [Test]
        public void RenameObjects_MultipleAssets_Renames()
        {
            // Arrange
            var multipleObject0 = this.CreatePrefabFromGameObject(
                                      new GameObject("Asset0"),
                                      string.Concat(TestFixturesDirectory, "Asset0.prefab"));
            var multipleObject1 = this.CreatePrefabFromGameObject(
                                      new GameObject("Asset1"),
                                      string.Concat(TestFixturesDirectory, "Asset1.prefab"));
            var multipleAssets = new List<Object>()
            {
                multipleObject0,
                multipleObject1
            };

            var replaceStringOp = new ReplaceStringOperation();
            replaceStringOp.SearchString = "Asset";
            replaceStringOp.ReplacementString = "Thingy";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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
            var spriteSheetConfig = new SpriteSheetGenerationConfig(2, "Texture.png");
            spriteSheetConfig.NamePrefix = "Texture_Sprite";
            var textureWithSprites = this.SetupSpriteSheet(spriteSheetConfig);
            var replaceNameOp = new ReplaceNameOperation();
            replaceNameOp.NewName = "NewSprite";

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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
            var enumeratedObject0 = this.CreatePrefabFromGameObject(
                                        new GameObject("EnumeratedObject0"),
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject0.prefab"));
            var enumeratedObject1 = this.CreatePrefabFromGameObject(
                                        new GameObject("EnumeratedObject1"),
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject1.prefab"));
            var enumeratedObject2 = this.CreatePrefabFromGameObject(
                                        new GameObject("EnumeratedObject2"),
                                        string.Concat(TestFixturesDirectory, "EnumeratedObject2.prefab"));

            var enumeratedObjects = new List<Object>()
            {
                enumeratedObject0,
                enumeratedObject1,
                enumeratedObject2,
            };

            var removeCharactersOp = new RemoveCharactersOperation();
            var removeCharacterOptions = new RemoveCharactersOperation.RenameOptions();
            removeCharacterOptions.CharactersToRemove = "\\d";
            removeCharacterOptions.CharactersAreRegex = true;
            removeCharacterOptions.IsCaseSensitive = false;
            removeCharactersOp.SetOptions(removeCharacterOptions);

            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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
        public void RenameObjects_ChangeObjectToExistingObjectName_SkipsRename()
        {
            // Arrange
            var conflictingObject0 = this.CreatePrefabFromGameObject(
                new GameObject("ConflictingObject0"),
                string.Concat(TestFixturesDirectory, "ConflictingObject0.prefab"));
            var existingObject = this.CreatePrefabFromGameObject(
                new GameObject("ExistingObject"),
                string.Concat(TestFixturesDirectory, "ExistingObject.prefab"));

            var conflictingObjectsWithoutAllNamesChanging = new List<Object>();
            conflictingObjectsWithoutAllNamesChanging.Add(conflictingObject0);
            conflictingObjectsWithoutAllNamesChanging.Add(existingObject);

            var replaceFirstNameOp = new ReplaceStringOperation();
            replaceFirstNameOp.SearchString = "ConflictingObject0";
            replaceFirstNameOp.ReplacementString = "ExistingObject";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(replaceFirstNameOp);

            // Act and Assert
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(conflictingObjectsWithoutAllNamesChanging);
            var expectedName = "ConflictingObject0";
            Assert.AreEqual(expectedName, conflictingObject0.name);
        }

        [Test]
        public void RenameObjects_ChangeObjectToExistingObjectNameNotInRenameGroup_SkipsRename()
        {
            // Arrange
            var conflictingObject0 = this.CreatePrefabFromGameObject(
                new GameObject("ConflictingObject0"),
                string.Concat(TestFixturesDirectory, "ConflictingObject0.prefab"));
            this.CreatePrefabFromGameObject(
                new GameObject("ExistingObject"),
                string.Concat(TestFixturesDirectory, "ExistingObject.prefab"));

            var conflictingObjectsWithoutAllNamesChanging = new List<Object>();
            conflictingObjectsWithoutAllNamesChanging.Add(conflictingObject0);

            var replaceFirstNameOp = new ReplaceStringOperation();
            replaceFirstNameOp.SearchString = "ConflictingObject0";
            replaceFirstNameOp.ReplacementString = "ExistingObject";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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
            var conflictingObject0 = this.CreatePrefabFromGameObject(
                new GameObject("ConflictingObject0"),
                string.Concat(TestFixturesDirectory, "ConflictingObject0.prefab"));
            var existingObject = this.CreatePrefabFromGameObject(
                new GameObject("ExistingObject"),
                string.Concat(TestFixturesDirectory, "SubDirectory/ExistingObject.prefab"));

            var replaceFirstNameOp = new ReplaceStringOperation();
            replaceFirstNameOp.SearchString = "ConflictingObject0";
            replaceFirstNameOp.ReplacementString = "ExistingObject";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
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
            // Tests Issue #143: https://github.com/redbluegames/unity-mulligan-renamer/issues/143
            // Arrange
            var spriteSheetConfig = new SpriteSheetGenerationConfig(4, "Texture.png");
            var targetSpriteName = "Texture_Sprite1";
            spriteSheetConfig.NamePrefix = "Texture_Sprite";
            var textureWithSprites = this.SetupSpriteSheet(spriteSheetConfig);
            var replaceNameOp = new ReplaceStringOperation();
            replaceNameOp.SearchString = targetSpriteName;
            replaceNameOp.ReplacementString = "CoolSprite";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(replaceNameOp);

            var path = AssetDatabase.GetAssetPath(textureWithSprites);
            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            var allSprites = new List<Object>();
            Object targetSprite = null;
            foreach (var asset in allAssetsAtPath)
            {
                if (asset is Sprite)
                {
                    allSprites.Add(asset);
                    if (asset.name == targetSpriteName)
                    {
                        targetSprite = asset;
                    }
                }
            }

            // Act
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(new List<Object>() { targetSprite }, true);

            // Assert
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

            // In order to not depend on how these sprites are Loaded, we check Contains instead of comparing 
            // the lists directly
            Assert.That(resultingNames.Count, Is.EqualTo(expectedNames.Count));
            foreach (var name in resultingNames)
            {
                Assert.That(expectedNames.Contains(name), "Expected names did not contain name: " + name);
            }
        }

        [Test]
        public void RenameObjects_SpriteAndTexture_Renames()
        {
            // Tests Issue #139: https://github.com/redbluegames/unity-mulligan-renamer/issues/139
            // Arrange
            var spriteSheetConfig = new SpriteSheetGenerationConfig(1, "Texture.png");
            spriteSheetConfig.NamePrefix = "Texture_Sprite";
            var textureWithSprites = this.SetupSpriteSheet(spriteSheetConfig);
            var replaceNameOp = new ReplaceStringOperation();
            replaceNameOp.SearchString = "Texture";
            replaceNameOp.ReplacementString = "Cool";

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(replaceNameOp);

            var path = AssetDatabase.GetAssetPath(textureWithSprites);
            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);

            // Act
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(new List<Object>(allAssetsAtPath), true);

            // Assert
            var resultingNames = new List<string>();
            foreach (var asset in allAssetsAtPath)
            {
                resultingNames.Add(asset.name);
            }

            var expectedNames = new List<string>
            {
                "Cool",
                "Cool_Sprite1",
            };

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_SpritesheetWithNamesThatWillOverlap_Renames()
        {
            // Tests Issue #126: https://github.com/redbluegames/unity-mulligan-renamer/issues/126
            // Arrange
            var spriteSheetConfig = new SpriteSheetGenerationConfig(2, "Texture.png");
            spriteSheetConfig.NamePrefix = "Texture_Sprite";
            spriteSheetConfig.UseZeroBasedIndexing = true;
            var textureWithSprites = this.SetupSpriteSheet(spriteSheetConfig);
            var removeNumbersOp = new RemoveCharactersOperation();
            removeNumbersOp.SetOptionPreset(RemoveCharactersOperation.PresetID.Numbers);
            var enumerateOp = new EnumerateOperation();
            enumerateOp.StartingCount = 1;
            enumerateOp.Increment = 1;

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(removeNumbersOp);
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

            // Act
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(allSprites, true);

            // Assert
            var expectedNames = new List<string>
            {
                "Texture_Sprite1",
                "Texture_Sprite2",
                "Texture_Sprite3",
                "Texture_Sprite4",
            };

            var resultingNames = new List<string>();
            foreach (var sprite in allSprites)
            {
                resultingNames.Add(sprite.name);
            }

            Assert.AreEqual(expectedNames, resultingNames);
        }

        [Test]
        public void RenameObjects_SpritesheetsWithPrefixedNumbers_Renames()
        {
            // Tests Issue #163: https://github.com/redbluegames/unity-mulligan-renamer/issues/163
            // Arrange
            var spriteSheetConfig = new SpriteSheetGenerationConfig(2, "NumberedSprites.png");
            spriteSheetConfig.NamePrefix = "0_NumberedSprites";
            spriteSheetConfig.UseZeroBasedIndexing = true;
            var textureWithSprites = this.SetupSpriteSheet(spriteSheetConfig);
            var replaceStringOperation = new ReplaceStringOperation();
            replaceStringOperation.SearchString = "Numbered";
            replaceStringOperation.ReplacementString = string.Empty;

            var renameSequence = new RenameOperationSequence<IRenameOperation>();
            renameSequence.Add(replaceStringOperation);

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

            // Act
            var bulkRenamer = new BulkRenamer(renameSequence);
            bulkRenamer.RenameObjects(allSprites, true);

            // Assert
            var expectedNames = new List<string>
            {
                "0_Sprites0",
                "0_Sprites1",
                "0_Sprites2",
                "0_Sprites3",
            };

            var resultingNames = new List<string>();
            foreach (var sprite in allSprites)
            {
                resultingNames.Add(sprite.name);
            }

            // In order to not depend on how these sprites are Loaded, we check Contains instead of comparing 
            // the lists directly
            Assert.That(resultingNames.Count, Is.EqualTo(expectedNames.Count));
            foreach (var name in resultingNames)
            {
                Assert.That(expectedNames.Contains(name), "Expected names did not contain name: " + name);
            }
        }

        private Texture2D SetupSpriteSheet(SpriteSheetGenerationConfig config)
        {
            var cellSize = 32;
            var texture = new Texture2D(
                cellSize * config.CellsPerSide,
                cellSize * config.CellsPerSide,
                TextureFormat.ARGB32,
                false,
                true);
            var size = Vector2.one * cellSize * config.CellsPerSide;
            for (int x = 0; x < size.x; ++x)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    texture.SetPixel(x, y, Color.cyan);
                }
            }

            texture.Apply();

            // Need to save the texture as an Asset and store a reference to the Asset
            var path = string.Concat(TestFixturesDirectory, config.TextureName);
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            var textureWithSprites = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            var spriteMetaData = new SpriteMetaData[config.CellsPerSide * config.CellsPerSide];
            for (int i = 0; i < config.CellsPerSide; ++i)
            {
                for (int j = 0; j < config.CellsPerSide; ++j)
                {
                    var cellIndex = i * config.CellsPerSide + j;
                    var x = i * cellSize;
                    var y = j * cellSize;
                    spriteMetaData[cellIndex].rect = new Rect(x, y, cellSize, cellSize);
                    var spriteCount = config.UseZeroBasedIndexing ? cellIndex : cellIndex + 1;
                    var name = string.Concat(config.NamePrefix, spriteCount.ToString());
                    spriteMetaData[cellIndex].name = name;
                }
            }

            importer.spritesheet = spriteMetaData;
            importer.SaveAndReimport();

            return textureWithSprites;
        }

        private class SpriteSheetGenerationConfig
        {
            public int CellsPerSide { get; set; }
            public string TextureName { get; set; }
            public string NamePrefix { get; set; }
            public bool UseZeroBasedIndexing { get; set; }

            public SpriteSheetGenerationConfig(int cellsPerSide, string textureName)
            {
                this.CellsPerSide = cellsPerSide;
                this.TextureName = textureName;
            }
        }
    }
}
