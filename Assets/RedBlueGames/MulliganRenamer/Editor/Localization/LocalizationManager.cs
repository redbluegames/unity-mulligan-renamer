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
                return currentLanguage;
            }
        }

        public List<Language> AllLanguages
        {
            get
            {
                return allLanguages;
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
            var jsons = Resources.LoadAll<TextAsset>("MulliganLanguages");
            foreach (var json in jsons)
            {
                var language = JsonUtility.FromJson<Language>(json.text);
                if (!string.IsNullOrEmpty(language.Key))
                {
                    loadedLanguages.Add(language);
                }
            }

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

        private void CacheAllLanguages()
        {
            this.allLanguages = LoadAllLanguages();
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
    }
}