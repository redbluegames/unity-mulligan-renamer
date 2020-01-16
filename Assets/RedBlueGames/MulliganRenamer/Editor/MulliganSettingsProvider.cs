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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    static class MulliganSettingsProvider
    {

        // Settings providers were added in 2018.3. No support for older versions for now.
#if UNITY_2018_3_OR_NEWER
        private static MulliganUserPreferences ActivePreferences;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Red Blue Games/Mulligan Renamer", SettingsScope.User)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Mulligan",
                activateHandler = (searchContext, rootElement) =>
                {
                    ActivePreferences = MulliganUserPreferences.LoadOrCreatePreferences();
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
            var prefsChanged = false;
            GUILayout.Label("Diff Colors", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            ActivePreferences.InsertionTextColor = EditorGUILayout.ColorField("Insertion Text", ActivePreferences.InsertionTextColor);
            ActivePreferences.InsertionBackgroundColor = EditorGUILayout.ColorField("Insertion Background", ActivePreferences.InsertionBackgroundColor);
            ActivePreferences.DeletionTextColor = EditorGUILayout.ColorField("Deletion Text", ActivePreferences.DeletionTextColor);
            ActivePreferences.DeletionBackgroundColor = EditorGUILayout.ColorField("Deletion Background", ActivePreferences.DeletionBackgroundColor);
            if (EditorGUI.EndChangeCheck())
            {
                prefsChanged = true;
            }

            if (GUILayout.Button("Reset to Default", GUILayout.Width(120)))
            {
                ActivePreferences.ResetColorsToDefault(EditorGUIUtility.isProSkin);
                prefsChanged = true;
            }

            if (prefsChanged)
            {
                ActivePreferences.SaveToEditorPrefs();
            }
        }
    }
#endif
}