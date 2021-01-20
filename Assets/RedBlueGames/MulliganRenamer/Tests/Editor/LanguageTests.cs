/* MIT License

Copyright (c) 2020 Murillo Pugliesi Lopes, https://github.com/Mukarillo

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
    using System.Collections.Generic;
    using NUnit.Framework;

    public class LanguageTests
    {
        private List<Language> allResourceLanguages;

        [SetUp]
        public void Init()
        {
            this.allResourceLanguages = LocalizationManager.LoadAllLanguages();
        }

        [Test]
        public void CheckAllLanguages_ForValidJSON()
        {
	        var jsons = UnityEngine.Resources.LoadAll<UnityEngine.TextAsset>("MulliganLanguages");
	        foreach (var json in jsons)
	        {
		        Assert.DoesNotThrow(() => UnityEngine.JsonUtility.FromJson<Language>(json.text)); 
	        }
        }

        [Test]
        public void CheckAllLanguages_ForNameKey()
        {
            var jsons = UnityEngine.Resources.LoadAll<UnityEngine.TextAsset>("MulliganLanguages");
            foreach (var json in jsons)
            {
                var language = UnityEngine.JsonUtility.FromJson<Language>(json.text);
                Assert.That(
                    !string.IsNullOrEmpty(language.Name),
                    "Name element for the language in json file named: " + json.name + " was null or empty. Is the Key named correctly?");
                Assert.That(
                    !string.IsNullOrEmpty(language.Key),
                    "Key element for the language in json file named: " + json.name + " was null or empty. Is the Key named correctly?");
            }
        }

        [Test]
        public void CheckAllLanguages_ForMatchingEnglishElement()
        {
            var englishLanguage = this.allResourceLanguages.Find(x => x.Key == "en");

            foreach (var language in this.allResourceLanguages)
            {
                if (language.Key == "en")
                    continue;

                foreach (var englishElement in englishLanguage.Elements)
                {
                    Assert.That(
                        language.Elements.Exists(x => x.Key == englishElement.Key),
                        "Language " + language.Name + " does not contain key \"" + englishElement.Key +
                        "\". Please add an element with this key to the language.");
                }
            }
        }

        [Test]
        public void CheckAllLanguages_HasValue()
        {
            foreach (var language in this.allResourceLanguages)
            {
                foreach (var element in language.Elements)
                {
                    Assert.That(!string.IsNullOrEmpty(element.Value),
                        "Language " + language.Name + " does not contain a value for key \"" + element.Key +
                        "\". Please add a value to the language.");
                }
            }
        }

        [Test]
        public void AllKeysInProject_ExistInAllLanguages()
        {
            var allScriptsGUIDs = UnityEditor.AssetDatabase.FindAssets("t:script");
            var keysInUse = new List<ScriptKeyPair>();
            var regex = new System.Text.RegularExpressions.Regex("LocaleManager.Instance.GetTranslation\\(\"([^\\)]*)\"\\)[,;]");
            foreach (var guid in allScriptsGUIDs)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var script = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(path);
                var matches = regex.Matches(script.text);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var key = match.Groups[1];
                    keysInUse.Add(new ScriptKeyPair() { ScriptName = script.name, Key = key.Value });
                }
            }

            foreach (var language in this.allResourceLanguages)
            {
                foreach (var scriptKeyPair in keysInUse)
                {
                    Assert.That(
                        language.Elements.Exists(x => x.Key == scriptKeyPair.Key),
                        "Script \"" + scriptKeyPair.ScriptName + "\" tries to access key \"" + scriptKeyPair.Key +
                        "\" from Language " + language.Name + " but it could not be found." +
                        "Please add an element with this key to the language, or remove the GetTranslation call.");
                }
            }
        }

        private class ScriptKeyPair
        {
            public string ScriptName { get; set; }

            public string Key { get; set; }
        }
    }
}