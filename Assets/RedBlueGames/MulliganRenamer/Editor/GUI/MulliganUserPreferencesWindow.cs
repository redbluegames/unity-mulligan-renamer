/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Handles drawing the PreferencesWindow in a generic way that works both in the
    /// SettingsProvider version of Preferences (native Unity Preferences menu), as well as
    /// a custom window for older versions.
    /// </summary>
    public class MulliganUserPreferencesWindow : EditorWindow
    {
        private static GUIStyle sampleDiffLabelStyle;

        private const float LabelWidth = 200.0f;

        private const float MaxWidth = 550.0f;

        private MulliganUserPreferences ActivePreferences;


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

        public static MulliganUserPreferencesWindow ShowWindow()
        {
            return EditorWindow.GetWindow<MulliganUserPreferencesWindow>(
                true,
                LocaleManager.Instance.GetTranslation("preferenceWindowTitle"),
                true);
        }

        private void OnEnable()
        {
            ActivePreferences = MulliganUserPreferences.LoadOrCreatePreferences();
        }

        private void OnGUI()
        {
            DrawPreferences(this.ActivePreferences);
        }

        /// <summary>
        /// Draw the Preferences using Unity GUI framework.
        /// </summary>
        /// <param name="preferences">Preferences to draw and update</param>
        public static void DrawPreferences(MulliganUserPreferences preferences)
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

            GUILayout.Label(LocaleManager.Instance.GetTranslation("preferencesDiffLabel"), EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            preferences.InsertionTextColor = EditorGUILayout.ColorField(
                LocaleManager.Instance.GetTranslation("preferencesInsertionText"),
                preferences.InsertionTextColor,
                GUILayout.MaxWidth(MaxWidth));
            preferences.InsertionBackgroundColor = EditorGUILayout.ColorField(
                LocaleManager.Instance.GetTranslation("preferencesInsertionBackground"),
                preferences.InsertionBackgroundColor,
                GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(true, preferences);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            preferences.DeletionTextColor = EditorGUILayout.ColorField(
                LocaleManager.Instance.GetTranslation("preferencesDeletionText"),
                preferences.DeletionTextColor,
                GUILayout.MaxWidth(MaxWidth));
            preferences.DeletionBackgroundColor = EditorGUILayout.ColorField(
                LocaleManager.Instance.GetTranslation("preferencesDeletionBackground"),
                preferences.DeletionBackgroundColor,
                GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(false, preferences);

            if (EditorGUI.EndChangeCheck())
            {
                prefsChanged = true;
            }

            if (GUILayout.Button(LocaleManager.Instance.GetTranslation("preferencesReset"), GUILayout.Width(150)))
            {
                preferences.ResetColorsToDefault(EditorGUIUtility.isProSkin);
                prefsChanged = true;
            }

            if (prefsChanged)
            {
                preferences.SaveToEditorPrefs();
            }
        }

        private static LocaleLanguage DrawLanguageDropdown(LocaleLanguage currentLanguage)
        {
            var content = new GUIContent(
                LocaleManager.Instance.GetTranslation("language"),
                LocaleManager.Instance.GetTranslation(" languageTooltip"));
            var languages = new GUIContent[LocaleManager.Instance.AllLanguages.Count];
            for (int i = 0; i < LocaleManager.Instance.AllLanguages.Count; ++i)
            {
                var language = LocaleManager.Instance.AllLanguages[i];
                languages[i] = new GUIContent(language.LanguageName);
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

        private static void DrawSampleDiffLabel(bool insertion, MulliganUserPreferences preferences)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(string.Empty, GUILayout.MaxWidth(LabelWidth));
            var rect = EditorGUILayout.GetControlRect();
            if (insertion)
            {
                DrawSampleInsertionLabel(rect, preferences);
            }
            else
            {
                DrawSampleDeletionLabel(rect, preferences);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawSampleInsertionLabel(Rect rect, MulliganUserPreferences preferences)
        {
            var diffLabelStyle = new MulliganEditorGUIUtilities.DiffLabelStyle()
            {
                HideDiff = false,
                OperationToShow = DiffOperation.Insertion,
                DiffBackgroundColor = preferences.InsertionBackgroundColor,
                DiffTextColor = preferences.InsertionTextColor,
            };

            var renameResult = new RenameResult();
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleThisIs") + " ", DiffOperation.Equal));
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleSampleText"), DiffOperation.Insertion));
            renameResult.Add(new Diff(" " + LocaleManager.Instance.GetTranslation("exampleWithWords") + " ", DiffOperation.Equal));
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleInserted"), DiffOperation.Insertion));

            MulliganEditorGUIUtilities.DrawDiffLabel(rect, renameResult, false, diffLabelStyle, SampleDiffLabelStyle);
        }

        private static void DrawSampleDeletionLabel(Rect rect, MulliganUserPreferences preferences)
        {
            var diffLabelStyle = new MulliganEditorGUIUtilities.DiffLabelStyle()
            {
                HideDiff = false,
                OperationToShow = DiffOperation.Deletion,
                DiffBackgroundColor = preferences.DeletionBackgroundColor,
                DiffTextColor = preferences.DeletionTextColor,
            };

            var renameResult = new RenameResult();
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleThisIs") + " ", DiffOperation.Equal));
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleSampleText"), DiffOperation.Deletion));
            renameResult.Add(new Diff(" " + LocaleManager.Instance.GetTranslation("exampleWithWords") + " ", DiffOperation.Equal));
            renameResult.Add(new Diff(LocaleManager.Instance.GetTranslation("exampleDeleted"), DiffOperation.Deletion));

            MulliganEditorGUIUtilities.DrawDiffLabel(rect, renameResult, true, diffLabelStyle, SampleDiffLabelStyle);
        }
    }
}