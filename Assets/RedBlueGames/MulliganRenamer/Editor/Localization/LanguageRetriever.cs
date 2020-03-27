namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LanguageRetriever
    {
        public bool IsDoneUpdating { get; private set; }

        public LanguageRetriever()
        {
            this.IsDoneUpdating = true;
        }

        public void UpdateLanguages()
        {
            Debug.Log("Starting Update");
            EditorCoroutineUtility.StartBackgroundTask(this.UpdateLanguagesAsync(), this.HandleUpdateComplete);
        }

        private IEnumerator UpdateLanguagesAsync()
        {
            this.IsDoneUpdating = false;

            LanguageBookmarks bookmarks = null;
            {
                var bookmarkRetriever = new JSONRetrieverWeb<LanguageBookmarks>
                    ("https://raw.githubusercontent.com/redbluegames/unity-mulligan-renamer/languages-from-web-tested/LanguageBookmarks.json");
                var bookmarkFetchOp = bookmarkRetriever.GetJSON(3);
                while (bookmarkFetchOp.Status == AsyncStatus.Pending)
                {
                    yield return null;
                }

                if (bookmarkFetchOp.Status != AsyncStatus.Success)
                {
                    Debug.Log("Whoops. Status:" + bookmarkFetchOp.Status + ". FailCode:" + bookmarkFetchOp.FailureCode + ". Message" + bookmarkFetchOp.FailureMessage);
                    yield break;
                }

                bookmarks = bookmarkFetchOp.ResultData;
            }

            var languages = new List<Language>();
            {
                foreach (var url in bookmarks.LanguageUrls)
                {
                    var languageRetriever = new JSONRetrieverWeb<Language>(url);
                    var languageFetchOp = languageRetriever.GetJSON(3);
                    while (languageFetchOp.Status == AsyncStatus.Pending)
                    {
                        yield return null;
                    }

                    if (languageFetchOp.Status != AsyncStatus.Success)
                    {
                        Debug.Log("Whoops. Status:" + languageFetchOp.Status + ". FailCode:" + languageFetchOp.FailureCode + ". Message" + languageFetchOp.FailureMessage);
                        continue;
                    }

                    languages.Add(languageFetchOp.ResultData);
                }
            }

            foreach (var language in languages)
            {
                this.UpdateLanguage(language);
                Debug.Log("Language: " + language.Name);
            }
        }

        private void HandleUpdateComplete()
        {
            Debug.Log("Update Complete");
            this.IsDoneUpdating = true;
        }

        private void UpdateLanguage(Language language)
        {
            // find the corresponding language on disk
            // LocaleManager.Instance.AllLanguages;
            // If it doesn't, add this one.
            // .  LocaleManager.Instance.SaveLanguageToDisk(Locale language) (needs to load it immediately)
            // compare the two
            // If new one is newer version, stomp old one. Otherwise nothing.
        }
    }
}