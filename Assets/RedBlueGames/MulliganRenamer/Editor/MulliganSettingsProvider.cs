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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    static class MulliganSettingsProvider
    {
        public static bool ArePreferencesImplemented
        {
            get
            {
#if MULLIGAN_INCLUDE_PREFS
                return true;
#else
                return false;
#endif
            }
        }

        public static string Path
        {
            get
            {
#if MULLIGAN_INCLUDE_PREFS
                return "Red Blue Games/Mulligan Renamer";
#else
                return string.Empty;
#endif
            }
        }

        // Settings providers were added in 2018.3. No support for older versions for now.
#if MULLIGAN_INCLUDE_PREFS

        private const float LabelWidth = 200.0f;
        private const float MaxWidth = 550.0f;

        private static MulliganUserPreferences ActivePreferences;

        private static GUIStyle sampleDiffLabelStyle;

        private static GUIStyle SampleDiffLabelStyle
        {
            get
            {
                if (sampleDiffLabelStyle == null)
                {
                    sampleDiffLabelStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
                }

                return sampleDiffLabelStyle;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider(Path, SettingsScope.User)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Mulligan Renamer",
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
            // I override LabelWidth (and MaxWidth) just to look more like Unity's native preferences
            EditorGUIUtility.labelWidth = LabelWidth;

            var prefsChanged = false;
            var newLanguage = DrawLanguageDropdown(LocaleManager.Instance.CurrentLanguage);
            if (newLanguage != LocaleManager.Instance.CurrentLanguage)
            {
                LocaleManager.Instance.ChangeLocale(newLanguage.LanguageKey);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label("Diff Colors", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            ActivePreferences.InsertionTextColor = EditorGUILayout.ColorField("Insertion Text", ActivePreferences.InsertionTextColor, GUILayout.MaxWidth(MaxWidth));
            ActivePreferences.InsertionBackgroundColor = EditorGUILayout.ColorField("Insertion Background", ActivePreferences.InsertionBackgroundColor, GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(true);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            ActivePreferences.DeletionTextColor = EditorGUILayout.ColorField("Deletion Text", ActivePreferences.DeletionTextColor, GUILayout.MaxWidth(MaxWidth));
            ActivePreferences.DeletionBackgroundColor = EditorGUILayout.ColorField("Deletion Background", ActivePreferences.DeletionBackgroundColor, GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(false);

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

        private static LocaleLanguage DrawLanguageDropdown(LocaleLanguage currentLanguage)
        {
            var content = new GUIContent(
                LocaleManager.Instance.GetTranslation("language"),
                "Specifies the language for all text used in Mulligan.");
            var languages = new string[LocaleManager.Instance.AllLanguages.Count];
            for (int i = 0; i < LocaleManager.Instance.AllLanguages.Count; ++i)
            {
                var language = LocaleManager.Instance.AllLanguages[i];
                languages[i] = language.LanguageName;
            }

            var currentLanguageIndex = GetLanguageIndex(currentLanguage);
            var newIndex = EditorGUILayout.Popup(content, currentLanguageIndex, languages, GUILayout.MaxWidth(MaxWidth));
            return LocaleManager.Instance.AllLanguages[newIndex];
        }

        private static int GetLanguageIndex(LocaleLanguage language)
        {
            var currentLanguageIndex = -1;
            for (int i = 0; i < LocaleManager.Instance.AllLanguages.Count; ++i)
            {
                if (LocaleManager.Instance.AllLanguages[i] == language)
                {
                    currentLanguageIndex = i;
                    break;
                }
            }

            return currentLanguageIndex;
        }

        private static void DrawSampleDiffLabel(bool insertion)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(string.Empty, GUILayout.MaxWidth(LabelWidth));
            var rect = EditorGUILayout.GetControlRect();
            if (insertion)
            {
                DrawSampleInsertionLabel(rect);
            }
            else
            {
                DrawSampleDeletionLabel(rect);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawSampleInsertionLabel(Rect rect)
        {
            var diffLabelStyle = new MulliganEditorGUIUtilities.DiffLabelStyle()
            {
                HideDiff = false,
                OperationToShow = DiffOperation.Insertion,
                DiffBackgroundColor = ActivePreferences.InsertionBackgroundColor,
                DiffTextColor = ActivePreferences.InsertionTextColor,
            };

            var renameResult = new RenameResult();
            renameResult.Add(new Diff("This is ", DiffOperation.Equal));
            renameResult.Add(new Diff("sample text", DiffOperation.Insertion));
            renameResult.Add(new Diff(" with words ", DiffOperation.Equal));
            renameResult.Add(new Diff("Inserted", DiffOperation.Insertion));

            MulliganEditorGUIUtilities.DrawDiffLabel(rect, renameResult, false, diffLabelStyle, SampleDiffLabelStyle);
        }

        private static void DrawSampleDeletionLabel(Rect rect)
        {
            var diffLabelStyle = new MulliganEditorGUIUtilities.DiffLabelStyle()
            {
                HideDiff = false,
                OperationToShow = DiffOperation.Deletion,
                DiffBackgroundColor = ActivePreferences.DeletionBackgroundColor,
                DiffTextColor = ActivePreferences.DeletionTextColor,
            };

            var renameResult = new RenameResult();
            renameResult.Add(new Diff("This is ", DiffOperation.Equal));
            renameResult.Add(new Diff("sample text", DiffOperation.Deletion));
            renameResult.Add(new Diff(" with words ", DiffOperation.Equal));
            renameResult.Add(new Diff("Deleted", DiffOperation.Deletion));

            MulliganEditorGUIUtilities.DrawDiffLabel(rect, renameResult, true, diffLabelStyle, SampleDiffLabelStyle);
        }
#endif
    }
}
