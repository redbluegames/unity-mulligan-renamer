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

            var timeStarted = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - timeStarted < 3.0f)
            {
                yield return null;
            }

            // Fetch Languages
            // . Foreach valid language
            // .    Update it in the project
            // .       If the fetched one is newer, Update it. If it's older, do nothing.
            // . Foreach removed Language
            // .    Delete it
        }

        private void HandleUpdateComplete()
        {
            Debug.Log("Update Complete");
            this.IsDoneUpdating = true;
        }

        private List<Locale> FetchLanguages()
        {
            Debug.Log("Fetching Languages");
            // Get Bookmarks
            // If Bookmarks is valid
            //   Foreach Bookmark
            // .    Download Language at Bookmark
            return null;
        }

        private void UpdateLanguage(Locale localeA)
        {
            // find the corresponding language on disk
            // LocaleManager.Instance.AllLanguages;
            // If it doesn't, add this one.
            // .  LocaleManager.Instance.SaveLanguageToDisk(Locale language)
            // compare the two

        }
    }
}