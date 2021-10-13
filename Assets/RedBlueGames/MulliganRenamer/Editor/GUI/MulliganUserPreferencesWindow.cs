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
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

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

        private MulliganUserPreferences activePreferences;

        private LanguageRetriever languageRetriever;

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
                LocalizationManager.Instance.GetTranslation("preferenceWindowTitle"),
                true);
        }

        private void OnEnable()
        {
            // Note that Enable is not called when opened as Preference item (via SettingsProvider api)
            // We implement it for old versions of Unity that just use a traditional EditorWindow for settings
            this.activePreferences = MulliganUserPreferences.LoadOrCreatePreferences();
            this.languageRetriever = new LanguageRetriever();
        }

        private void OnGUI()
        {
            DrawPreferences(this.activePreferences, this.languageRetriever);
        }

        /// <summary>
        /// Draw the Preferences using Unity GUI framework.
        /// </summary>
        /// <param name="preferences">Preferences to draw and update</param>
        public static void DrawPreferences(MulliganUserPreferences preferences, LanguageRetriever languageRetriever)
        {
            // I override LabelWidth (and MaxWidth) just to look more like Unity's native preferences
            EditorGUIUtility.labelWidth = LabelWidth;

            var prefsChanged = false;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(MaxWidth));
            var newLanguage = DrawLanguageDropdown(LocalizationManager.Instance.CurrentLanguage);
            if (newLanguage != LocalizationManager.Instance.CurrentLanguage)
            {
                preferences.CurrentLanguageKey = newLanguage.Key;
                LocalizationManager.Instance.ChangeLanguage(newLanguage.Key);
            }

            DrawUpdateLanguagesButton(languageRetriever);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label(LocalizationManager.Instance.GetTranslation("preferencesDiffLabel"), EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            preferences.InsertionTextColor = EditorGUILayout.ColorField(
                LocalizationManager.Instance.GetTranslation("preferencesInsertionText"),
                preferences.InsertionTextColor,
                GUILayout.MaxWidth(MaxWidth));
            preferences.InsertionBackgroundColor = EditorGUILayout.ColorField(
                LocalizationManager.Instance.GetTranslation("preferencesInsertionBackground"),
                preferences.InsertionBackgroundColor,
                GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(true, preferences);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            preferences.DeletionTextColor = EditorGUILayout.ColorField(
                LocalizationManager.Instance.GetTranslation("preferencesDeletionText"),
                preferences.DeletionTextColor,
                GUILayout.MaxWidth(MaxWidth));
            preferences.DeletionBackgroundColor = EditorGUILayout.ColorField(
                LocalizationManager.Instance.GetTranslation("preferencesDeletionBackground"),
                preferences.DeletionBackgroundColor,
                GUILayout.MaxWidth(MaxWidth));
            EditorGUILayout.Space();
            DrawSampleDiffLabel(false, preferences);

            if (EditorGUI.EndChangeCheck())
            {
                prefsChanged = true;
            }

            if (GUILayout.Button(LocalizationManager.Instance.GetTranslation("preferencesReset"), GUILayout.Width(150)))
            {
                preferences.ResetColorsToDefault(EditorGUIUtility.isProSkin);
                prefsChanged = true;
            }

            if (prefsChanged)
            {
                preferences.SaveToEditorPrefs();
            }
        }

        private static void DrawUpdateLanguagesButton(LanguageRetriever retriever)
        {
            EditorGUI.BeginDisabledGroup(!retriever.IsDoneUpdating);
            var useDebugPresets = Event.current.shift;
            var buttonText = LocalizationManager.Instance.GetTranslation("updateLanguages");
            if (useDebugPresets)
            {
                buttonText = string.Concat(buttonText, "*");
            }

            if (GUILayout.Button(buttonText))
            {
                retriever.UpdateLanguages(useDebugPresets);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static Language DrawLanguageDropdown(Language currentLanguage)
        {
            var content = new GUIContent(
                LocalizationManager.Instance.GetTranslation("language"),
                LocalizationManager.Instance.GetTranslation(" languageTooltip"));
            var languages = new GUIContent[LocalizationManager.Instance.AllLanguages.Count];
            for (int i = 0; i < LocalizationManager.Instance.AllLanguages.Count; ++i)
            {
                var language = LocalizationManager.Instance.AllLanguages[i];
                languages[i] = new GUIContent(language.Name);
            }

            var currentLanguageIndex = GetLanguageIndex(currentLanguage);
            if (currentLanguageIndex >= 0 && currentLanguageIndex < LocalizationManager.Instance.AllLanguages.Count)
            {
                var newIndex = EditorGUILayout.Popup(content, currentLanguageIndex, languages, GUILayout.MaxWidth(MaxWidth));
                return LocalizationManager.Instance.AllLanguages[newIndex];
            }
            else
            {
                Debug.Log("Can't draw LanguageDropdown as the CurrentLanguage was not found in LocalizationManager." +
                    " Restarting Unity should fix this. This should not happen in production, please report it on GitHub issues.");
                return LocalizationManager.Instance.CurrentLanguage;
            }
        }

        private static int GetLanguageIndex(Language language)
        {
            var currentLanguageIndex = -1;
            for (int i = 0; i < LocalizationManager.Instance.AllLanguages.Count; ++i)
            {
                if (LocalizationManager.Instance.AllLanguages[i].Key == language.Key)
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

            var renameResult = CreateSampleTextForDiffOp(new string[] {"exampleSampleText", "exampleInserted"}, DiffOperation.Insertion);
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

            var renameResult = CreateSampleTextForDiffOp(new string[] {"exampleSampleText", "exampleDeleted"}, DiffOperation.Deletion);
            MulliganEditorGUIUtilities.DrawDiffLabel(rect, renameResult, true, diffLabelStyle, SampleDiffLabelStyle);
        }

        private static RenameResult CreateSampleTextForDiffOp(string[] keys, DiffOperation diffOp)
        {
            var renameResult = new RenameResult();
            string translatedText = LocalizationManager.Instance.GetTranslation("exampleTextWithInsertedWords");
            Regex regex = new Regex(@"{+\d+}+");
            MatchCollection matches = regex.Matches(translatedText);
            List<Diff> subStrings = new List<Diff>();

            for(int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                subStrings.Add(new Diff(translatedText.Substring(0, translatedText.IndexOf(match.Value)), DiffOperation.Equal));

                var stringToInsert = i >= 0 && i < keys.Length ? LocalizationManager.Instance.GetTranslation(keys[i]) : "modified";
                subStrings.Add(new Diff(stringToInsert, diffOp));
                translatedText = translatedText.Remove(0, translatedText.IndexOf(match.Value) + match.Value.Length);
            }

            foreach (Diff currentString in subStrings)
            {
                renameResult.Add(currentString);
            }

            return renameResult;
        }
    }
}