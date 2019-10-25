/* MIT License

Copyright (c) 2019 Murillo Pugliesi Lopes, https://github.com/Mukarillo

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
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// At the constructor we load the the previous language that the user used
    /// using english as the default. This will cache the language and it's translations
    /// also we load all the languages by accesing the JSON files inside LocaleLanguagesPath path
    /// this is used to show the available languages to the user
    /// </summary>
    public class LocaleManager
    {
        private static LocaleManager _Instance;

        private const string LocaleKey = "RedBlueGames.MulliganRenamer.Locale";

        public UnityEvent OnLanguageChanged = new UnityEvent();

        public static LocaleManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new LocaleManager();
                }

                return _Instance;
            }
        }

        public LocaleLanguage CurrentLanguage
        {
            get
            {
                return currentLanguage;
            }
        }

        public List<LocaleLanguage> AllLanguages
        {
            get
            {
                return allLanguages;
            }
        }

        private LocaleLanguage currentLanguage;
        private List<LocaleLanguage> allLanguages;

        public LocaleManager()
        {
            this.LoadAllLanguages();
            this.ChangeLocale(EditorPrefs.GetString(LocaleKey, "en"));
        }

        private void LoadAllLanguages()
        {
            allLanguages = new List<LocaleLanguage>();

            var jsons = Resources.LoadAll<TextAsset>("Content");
            foreach (var json in jsons)
            {
                var language = JsonUtility.FromJson<LocaleLanguage>(json.text);
                if (!string.IsNullOrEmpty(language.LanguageKey))
                    allLanguages.Add(language);
            }
        }

        public void ChangeLocale(string languageKey)
        {
            EditorPrefs.SetString(LocaleKey, languageKey);
            this.currentLanguage = allLanguages.Find(x => x.LanguageKey == languageKey);
            if (OnLanguageChanged != null)
                OnLanguageChanged.Invoke();
        }

        public string GetTranslation(string localeKey)
        {
            if (this.currentLanguage == null)
                throw new Exception("Current Language is not set");

            return this.currentLanguage.GetValue(localeKey);
        }
    }
}