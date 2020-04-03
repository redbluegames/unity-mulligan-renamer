/* MIT License

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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class is responsible for getting, or retrieving, the up to date languages from the web.
    /// </summary>
    public class LanguageRetriever
    {
        private static readonly string BookmarksURIRelease = "https://raw.githubusercontent.com/redbluegames/unity-mulligan-renamer/master/LanguageBookmarks.json";

        private static readonly string BookmarksURIStaging = "https://raw.githubusercontent.com/redbluegames/unity-mulligan-renamer/staging/LanguageBookmarks.json";

        public bool IsDoneUpdating { get; private set; }

        public LanguageRetriever()
        {
            this.IsDoneUpdating = true;
        }

        public void UpdateLanguages(bool useStagingLink = false)
        {
            EditorCoroutineUtility.StartBackgroundTask(this.UpdateLanguagesAsync(useStagingLink), this.HandleUpdateComplete);
        }

        private IEnumerator UpdateLanguagesAsync(bool useStagingLink = false)
        {
            EditorUtility.DisplayProgressBar(
                LocalizationManager.Instance.GetTranslation("languageUpdateProgressTitle"),
                LocalizationManager.Instance.GetTranslation("languageUpdateProgressMessage1"),
                0.0f);
            this.IsDoneUpdating = false;

            LanguageBookmarks bookmarks = null;
            {
                var bookmarkRetriever = new JSONRetrieverWeb<LanguageBookmarks>(useStagingLink ? BookmarksURIStaging : BookmarksURIRelease);
                var bookmarkFetchOp = bookmarkRetriever.GetJSON(3);
                while (bookmarkFetchOp.Status == AsyncStatus.Pending)
                {
                    yield return null;
                }

                if (bookmarkFetchOp.Status != AsyncStatus.Success)
                {
                    ShowDisplayDialogForFailedOp(bookmarkFetchOp);
                    yield break;
                }

                bookmarks = bookmarkFetchOp.ResultData;
            }

            var languages = new List<Language>();
            {
                for (int i = 0; i < bookmarks.LanguageUrls.Count; ++i)
                {
                    var url = bookmarks.LanguageUrls[i];
                    var uri = new System.Uri(bookmarks.LanguageUrls[i]);
                    string filename = System.IO.Path.GetFileName(uri.LocalPath);

                    // Add one because we finished downloading Bookmarks.
                    var percentComplete = (i + 1) / (float)(bookmarks.LanguageUrls.Count + 1);
                    EditorUtility.DisplayProgressBar(
                        LocalizationManager.Instance.GetTranslation("languageUpdateProgressTitle"),
                        string.Format(
                            LocalizationManager.Instance.GetTranslation(
                                "languageUpdateDownloadingLanguages"),
                                filename),
                        percentComplete);

                    var languageRetriever = new JSONRetrieverWeb<Language>(url);
                    var languageFetchOp = languageRetriever.GetJSON(3);
                    while (languageFetchOp.Status == AsyncStatus.Pending)
                    {
                        yield return null;
                    }

                    if (languageFetchOp.Status != AsyncStatus.Success)
                    {
                        ShowDisplayDialogForFailedOp(languageFetchOp);
                        yield break;
                    }

                    languages.Add(languageFetchOp.ResultData);
                }
            }

            EditorUtility.DisplayProgressBar(
                LocalizationManager.Instance.GetTranslation("languageUpdateProgressTitle"),
                LocalizationManager.Instance.GetTranslation("languageUpdateSavingChanges"),
                1.0f);
            EditorUtility.ClearProgressBar();

            var reports = LocalizationManager.Instance.AddOrUpdateLanguages(languages);
            EditorUtility.DisplayDialog(
                LocalizationManager.Instance.GetTranslation("languageUpdateProgressTitleSuccess"),
                BuildDisplayStringForReport(reports),
                LocalizationManager.Instance.GetTranslation("ok"));
        }

        private static void ShowDisplayDialogForFailedOp(AsyncOp op)
        {
            var message = BuildDisplayStringForAsyncOp(op);
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(
                LocalizationManager.Instance.GetTranslation("languageUpdateProgressTitleFail"),
                message,
                LocalizationManager.Instance.GetTranslation("ok"));
        }

        private static string BuildDisplayStringForAsyncOp(AsyncOp op)
        {
            string message = string.Empty;
            if (op.Status == AsyncStatus.Timeout)
            {
                message = LocalizationManager.Instance.GetTranslation("languageUpdateTimeout");
            }
            else if (op.Status == AsyncStatus.Failed)
            {
                message = string.Format(
                    LocalizationManager.Instance.GetTranslation("languageUpdateFail"),
                    op.FailureCode,
                    op.FailureMessage);
            }
            else
            {
                // Nothing to display for success or otherwise
            }

            return message;
        }

        private static string BuildDisplayStringForReport(List<LocalizationManager.LanguageUpdateReport> reports)
        {
            var updatedStringBuilder = new System.Text.StringBuilder();
            var addedLanguageStringBuilder = new System.Text.StringBuilder();
            var unchangedStringBuilder = new System.Text.StringBuilder();
            foreach (var report in reports)
            {
                if (report.Result == LocalizationManager.LanguageUpdateReport.UpdateResult.Updated)
                {
                    if (updatedStringBuilder.Length > 0)
                    {
                        updatedStringBuilder.AppendLine();
                    }

                    updatedStringBuilder.AppendFormat(
                        string.Format(
                            LocalizationManager.Instance.GetTranslation("languageUpdated"),
                            report.Language.Name,
                            report.PreviousVersion,
                            report.NewVersion));
                }
                else if (report.Result == LocalizationManager.LanguageUpdateReport.UpdateResult.Added)
                {
                    if (addedLanguageStringBuilder.Length > 0)
                    {
                        addedLanguageStringBuilder.AppendLine();
                    }

                    addedLanguageStringBuilder.AppendFormat(
                        string.Format(
                            LocalizationManager.Instance.GetTranslation("languageAdded"),
                            report.Language.Name));
                }
                else
                {
                    if (unchangedStringBuilder.Length > 0)
                    {
                        unchangedStringBuilder.AppendLine();
                    }

                    unchangedStringBuilder.AppendFormat(
                        string.Format(
                            LocalizationManager.Instance.GetTranslation("languageUnchanged"),
                            report.Language.Name));
                }
            }

            var message = new System.Text.StringBuilder();
            if (addedLanguageStringBuilder.Length > 0)
            {
                message.Append(addedLanguageStringBuilder);
            }

            if (updatedStringBuilder.Length > 0)
            {
                if (message.Length > 0)
                {
                    message.AppendLine();
                }

                message.Append(updatedStringBuilder);
            }

            if (message.Length == 0)
            {
                message.Append(LocalizationManager.Instance.GetTranslation("languageAllUpToDate"));
            }
            else
            {
                message.AppendLine();
                message.Append(unchangedStringBuilder);
            }

            return message.ToString();
        }

        private void HandleUpdateComplete()
        {
            this.IsDoneUpdating = true;
        }
    }
}