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

                // Issue #252 - When updating from 1.6 to 1.7 with window open, Resources would be null on first frame.
                // This resulted in exception spam when trying to GetTranslation because no languages were loaded.
                // Now we set a flag so that we will continue to reload until we get languages.
                if (!_Instance.areLanguagesLoaded)
                {
                    _Instance.Initialize();
                }

                return _Instance;
            }
        }

        public Language CurrentLanguage
        {
            get
            {
                return this.GetLanguageByKey(currentLanguageKey);
            }
            set
            {
                if (value != null)
                {
                    this.currentLanguageKey = value.Key;
                }
            }
        }

        public List<Language> AllLanguages
        {
            get
            {
                return this.allLanguages;
            }
        }

        private string currentLanguageKey;

        private List<Language> allLanguages;

        private bool areLanguagesLoaded;

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
                try
                {
                    var language = JsonUtility.FromJson<Language>(json.text);
                    if (!string.IsNullOrEmpty(language.Key))
                    {
                        loadedLanguages.Add(language);
                    }
                }
                catch (ArgumentException)
                {
                     //I don't want to spam users with an error here, so we will just have to fail silently for now.
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
            if (this.areLanguagesLoaded)
            {
                var preferences = MulliganUserPreferences.LoadOrCreatePreferences();
                this.ChangeLanguage(preferences.CurrentLanguageKey);
            }
        }

        /// <summary>
        /// Change the current Locale so that Translations are of the new, specified languages
        /// </summary>
        /// <param name="languageKey">LanguageKey to change to</param>
        public void ChangeLanguage(string languageKey)
        {
            var language = this.GetLanguageByKey(languageKey);
            if (language != null)
            {
                this.CurrentLanguage = language;

                if (this.LanguageChanged != null)
                {
                    this.LanguageChanged.Invoke();
                }
            }
            else
            {
                throw new System.ArgumentException(
                    "Language with key [" + languageKey + "] not found in LocalizationManager's loaded languages. " +
                    "Are you sure it's a valid key? Did ChangeLanguage get called bofore LocalizationManager could load languages?",
                    "languageKey");
            }
        }

        /// <summary>
        /// Adds new languages and update existing ones that are new or newer versions, using the specified languages.
        /// </summary>
        /// <param name="languages">Languages to update</param>
        public List<LanguageUpdateReport> AddOrUpdateLanguages(IEnumerable<Language> languages)
        {
            // Need to reload the languages in the LocalizationManager so that we compare
            // the new languages against up to date ones. For example, if the user deletes a
            // language or adds their own in the same session. Mostly this is just a use-case in testing.
            this.Initialize();

            var languageUpdateReports = new List<LanguageUpdateReport>();

            foreach (var language in languages)
            {
                var report = LocalizationManager.Instance.UpdateLanguage(language);
                languageUpdateReports.Add(report);
            }

            // Resort the languages in case they got reshuffled
            SortLanguages(this.allLanguages);

            return languageUpdateReports;
        }

        /// <summary>
        /// Get the translated string for the specified key in the current language.
        /// </summary>
        /// <param name="languageKey">Key whose value we will retrieve</param>
        /// <returns>The value stored at the key in the current language</returns>
        public string GetTranslation(string languageKey)
        {
            if (!this.areLanguagesLoaded)
            {
                return string.Empty;
            }

            if (this.CurrentLanguage != null)
            {
                return this.CurrentLanguage.GetValue(languageKey);
            }
            else
            {
                throw new Exception("CurrentLanguage is unset on LocalizationManager. Somehow we are requesting translations " +
                    "before LocalizationManager succesfully loaded languages.");
            }
        }

        private void CacheAllLanguages()
        {
            this.areLanguagesLoaded = false;
            this.allLanguages = LoadAllLanguages();
            if (this.allLanguages != null && this.allLanguages.Count > 0)
            {
                this.areLanguagesLoaded = true;
            }
        }

        private LanguageUpdateReport UpdateLanguage(Language newLanguage)
        {
            var report = new LanguageUpdateReport();
            report.Language = newLanguage;
            Language existingLanguage = this.GetLanguageByKey(newLanguage.Key);
            if (existingLanguage == null)
            {
                report.Result = LanguageUpdateReport.UpdateResult.Added;
                this.SaveLanguageToDisk(newLanguage);
            }
            else if (newLanguage.Version > existingLanguage.Version)
            {
                report.Result = LanguageUpdateReport.UpdateResult.Updated;
                report.PreviousVersion = existingLanguage.Version;
                report.NewVersion = newLanguage.Version;
                this.SaveLanguageToDisk(newLanguage);
            }
            else
            {
                report.Result = LanguageUpdateReport.UpdateResult.NoChange;
            }

            return report;
        }

        private void SaveLanguageToDisk(Language language)
        {
            var directory = GetPathToLanguages();
            var json = JsonUtility.ToJson(language, true);
            var filename = string.Concat(language.Key, ".json");
            var path = System.IO.Path.Combine(directory, filename);

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

        private Language GetLanguageByKey(string languagekey)
        {
            return this.allLanguages.FirstOrDefault(x => x.Key == languagekey);
        }

        public class LanguageUpdateReport
        {
            public Language Language { get; set; }

            public UpdateResult Result { get; set; }

            public int PreviousVersion { get; set; }

            public int NewVersion { get; set; }

            public enum UpdateResult
            {
                NoChange,
                Updated,
                Added
            }
        }
    }
}