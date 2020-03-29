/* MIT License

Copyright (c) 2019 Murillo Pugliesi Lopes, https://github.com/Mukarillo,
and Edward Rowe.

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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Manages the loading and serving of languages. Use this to get translated strings
    /// for the user's current language.
    /// </summary>
    public class LocalizationManager
    {
        private static LocalizationManager _Instance;

        private const string LanguagePrefKey = "RedBlueGames.MulliganRenamer.Locale";

        private const string LanguageFoldername = "MulliganLanguages";

        public event System.Action LanguageChanged;

        public static LocalizationManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new LocalizationManager();
                }

                return _Instance;
            }
        }

        public Language CurrentLanguage
        {
            get
            {
                return this.currentLanguage;
            }
        }

        public List<Language> AllLanguages
        {
            get
            {
                return this.allLanguages;
            }
        }

        private Language currentLanguage;
        private List<Language> allLanguages;

        private LocalizationManager()
        {
            this.Initialize();
        }

        /// <summary>
        /// Gets all the languages stored in the project for Mulligan
        /// </summary>
        /// <returns>A List of LocaleLanguages, loaded from disk.</returns>
        public static List<Language> LoadAllLanguages()
        {
            var loadedLanguages = new List<Language>();
            var jsons = Resources.LoadAll<TextAsset>(LanguageFoldername);
            foreach (var json in jsons)
            {
                var language = JsonUtility.FromJson<Language>(json.text);
                if (!string.IsNullOrEmpty(language.Key))
                {
                    loadedLanguages.Add(language);
                }
            }

            // We may want control over the sorting, instead of just using the order they
            // are loaded in Resources.LoadAll
            SortLanguages(loadedLanguages);

            return loadedLanguages;
        }

        /// <summary>
        /// (Re)Initialize the LocaleManager. This loads the languages and sets the language to English, if unset.
        /// </summary>
        public void Initialize()
        {
            this.CacheAllLanguages();
            this.ChangeLanguage(EditorPrefs.GetString(LanguagePrefKey, "en"));
        }

        /// <summary>
        /// Change the current Locale so that Translations are of the new, specified languages
        /// </summary>
        /// <param name="languageKey">LanguageKey to change to</param>
        public void ChangeLanguage(string languageKey)
        {
            EditorPrefs.SetString(LanguagePrefKey, languageKey);
            this.currentLanguage = allLanguages.Find(x => x.Key == languageKey);
            if (this.LanguageChanged != null)
            {
                this.LanguageChanged.Invoke();
            }
        }

        /// <summary>
        /// Adds new languages and update existing ones that are new or newer versions, using the specified languages.
        /// </summary>
        /// <param name="languages">Languages to update</param>
        public void AddOrUpdateLanguages(IEnumerable<Language> languages)
        {
            // Need to reload the languages in the LocalizationManager so that we compare
            // the new languages against up to date ones. For example, if the user deletes a
            // language or adds their own in the same session. Mostly this is just a use-case in testing.
            this.Initialize();

            foreach (var language in languages)
            {
                LocalizationManager.Instance.UpdateLanguage(language);
            }

            // Resort the languages in case they got reshuffled
            SortLanguages(this.allLanguages);
        }

        private void UpdateLanguage(Language newLanguage)
        {
            Language existingLanguage = this.allLanguages.FirstOrDefault((l) => l.Key == newLanguage.Key);
            if (existingLanguage == null)
            {
                Debug.Log("Adding new language: " + newLanguage.Name);
                this.SaveLanguageToDisk(newLanguage);
            }
            else if (newLanguage.Version > existingLanguage.Version)
            {
                Debug.Log("Updating existing language: " + existingLanguage.Name);
                this.SaveLanguageToDisk(newLanguage);
            }
            else
            {
                Debug.Log("Found matching language: " + existingLanguage.Name +
                    ", but it's the same (or newer) version. Will leave it unchanged.");
            }

            // newLanguage is a new language instance, even if it's the same "language", so
            // we need to update the reference for the CurrentLanguage to the new one.
            // Note this is not really "changing languages" so we don't fire the callback or update PrefKey
            if (this.CurrentLanguage.Key == newLanguage.Key)
            {
                this.currentLanguage = newLanguage;
            }
        }

        /// <summary>
        /// Get the translated string for the specified key in the current language.
        /// </summary>
        /// <param name="languageKey">Key whose value we will retrieve</param>
        /// <returns>The value stored at the key in the current language</returns>
        public string GetTranslation(string languageKey)
        {
            if (this.currentLanguage == null)
            {
                throw new Exception("Current Language is not set");
            }

            return this.currentLanguage.GetValue(languageKey);
        }

        private void CacheAllLanguages()
        {
            this.allLanguages = LoadAllLanguages();
        }

        private void SaveLanguageToDisk(Language language)
        {
            var directory = GetPathToLanguages();
            var json = JsonUtility.ToJson(language, true);
            var filename = string.Concat(language.Key, ".json");
            var path = System.IO.Path.Combine(directory, filename);

            Debug.Log("Writing file at path: " + path);

            System.IO.File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);

            // We need to unload the language if it exists so that we don't have two versions loaded
            this.UnloadLanguage(language);

            this.allLanguages.Add(language);
        }

        private void UnloadLanguage(Language language)
        {
            for (int i = this.allLanguages.Count - 1; i >= 0; --i)
            {
                if (this.allLanguages[i].Key == language.Key)
                {
                    this.allLanguages.RemoveAt(i);
                }
            }
        }

        private static void SortLanguages(List<Language> languages)
        {
            languages.Sort(CompareLangauges);
        }

        private static int CompareLangauges(Language languageA, Language languageB)
        {
            return UnityEditor.EditorUtility.NaturalCompare(languageA.Key, languageB.Key);
        }

        private static string GetPathToLanguages()
        {
            var jsons = Resources.LoadAll<TextAsset>(LanguageFoldername);
            if (jsons == null || jsons.Length == 0)
            {
                // This would happen if a user deletes all their languages.
                return string.Empty;
            }

            var pathToFirstLanguage = AssetDatabase.GetAssetPath(jsons[0]);
            return System.IO.Path.GetDirectoryName(pathToFirstLanguage);
        }
    }
}