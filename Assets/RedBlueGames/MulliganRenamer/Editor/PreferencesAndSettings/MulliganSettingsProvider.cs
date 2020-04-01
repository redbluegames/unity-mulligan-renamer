#if UNITY_2018_3_OR_NEWER
#define MULLIGAN_INCLUDE_PREFS
#endif
/* MIT License

Copyright (c) 2020 Edward Rowe, RedBlueGames

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
    using UnityEditor;

    static class MulliganSettingsProvider
    {
        // Settings providers were added in 2018.3. No support for older versions for now.
#if MULLIGAN_INCLUDE_PREFS
        public static string Path
        {
            get
            {
                return "Red Blue Games/Mulligan Renamer";
            }
        }


        private static MulliganUserPreferences ActivePreferences;

        private static LanguageRetriever LanguageRetriever;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider(Path, SettingsScope.User)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = LocalizationManager.Instance.GetTranslation("preferencesMenuItem"),
                activateHandler = (searchContext, rootElement) =>
                {
                    ActivePreferences = MulliganUserPreferences.LoadOrCreatePreferences();
                    LanguageRetriever = new LanguageRetriever();
                },

                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = DrawPreferences,

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Diff", "Color" })
            };

            return provider;
        }

        private static void DrawPreferences(string searchContext)
        {
            // Pass in state into the Window since window doesn't have any when opened as a Preference item
            MulliganUserPreferencesWindow.DrawPreferences(ActivePreferences, LanguageRetriever);
        }
#endif
    }
}
