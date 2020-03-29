﻿/* MIT License

Copyright (c) 2020 Edward Rowe

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
    using UnityEngine;

    /// <summary>
    /// This class is responsible for getting, or retrieving, the up to date languages from the web.
    /// </summary>
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

            LocalizationManager.Instance.AddOrUpdateLanguages(languages);
        }

        private void HandleUpdateComplete()
        {
            Debug.Log("Update Complete");
            this.IsDoneUpdating = true;
        }
    }
}