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
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class CountByLetterOperationDrawer : RenameOperationDrawer<CountByLetterOperation>
    {
        private readonly GUIContent CustomSequenceContent = new GUIContent(
            LocalizationManager.Instance.GetTranslation("strings"),
            LocalizationManager.Instance.GetTranslation("theStringsOfLettersToAdd"));

        public CountByLetterOperationDrawer()
        {
            this.Initialize();
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return GetOperationPath("add", "countByLetter");
            }
        }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return LocalizationManager.Instance.GetTranslation("countByLetter");
            }
        }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public override Color32 HighlightColor
        {
            get
            {
                return this.AddColor;
            }
        }

        /// <summary>
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public override string ControlToFocus
        {
            get
            {
                return LocalizationManager.Instance.GetTranslation("format");
            }
        }

        private List<CountByLetterPresetGUI> GUIPresets { get; set; }

        private int SelectedModeIndex
        {
            get
            {
                if (this.RenameOperation == null)
                {
                    return 0;
                }
                else
                {
                    for (int i = 0; i < this.GUIPresets.Count; ++i)
                    {
                        if (this.GUIPresets[i].Preset == this.RenameOperation.Preset)
                        {
                            return i;
                        }
                    }

                    // Could not find a GUIPreset that uses the selected string sequence preset,
                    // Just fallback to 0
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(5);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var currentRectSplit = 0;
            var numLines = 5;

            var sequenceRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            var newlySelectedPreset = this.DrawSequenceSelection(sequenceRect, controlPrefix);

            var customSequenceRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            if (newlySelectedPreset.Preset != CountByLetterOperation.StringPreset.Custom)
            {
                this.DrawPlaceholderSequenceField(customSequenceRect, controlPrefix, newlySelectedPreset.SequenceToDisplay);
                this.RenameOperation.SetCountSequencePreset(newlySelectedPreset.Preset);
            }
            else
            {
                // Clear out the sequence when moving from a non-custom to Custom so that they don't
                // see it prepopulated with the previous preset's entries.
                if (this.RenameOperation.Preset != CountByLetterOperation.StringPreset.Custom)
                {
                    this.RenameOperation.SetCountSequence(new string[] { });
                }

                var customSequence = this.DrawCustomSequenceField(
                    customSequenceRect,
                    controlPrefix,
                    this.RenameOperation.CountSequence);
                this.RenameOperation.SetCountSequence(customSequence);
            }

            var countFromRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            this.RenameOperation.StartingCount = this.DrawCountFromField(
                countFromRect,
                controlPrefix,
                this.RenameOperation.StartingCount);

            var incrementRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            this.RenameOperation.Increment = this.DrawIncrementField(
                incrementRect,
                controlPrefix,
                this.RenameOperation.Increment);

            var prependRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            this.RenameOperation.Prepend = this.DrawPrependField(
                prependRect,
                controlPrefix,
                this.RenameOperation.Prepend);
        }

        private CountByLetterPresetGUI DrawSequenceSelection(Rect rect, int controlPrefix)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Format"));
            var modeContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("format"),
                LocalizationManager.Instance.GetTranslation("formatForTheAddedLetters"));
            var optionsContent = new GUIContent[this.GUIPresets.Count];
            for (int i = 0; i < optionsContent.Length; ++i)
            {
                optionsContent[i] = new GUIContent(this.GUIPresets[i].DisplayName);
            }

            var newlySelectedIndex = EditorGUI.Popup(
                rect,
                modeContent,
                this.SelectedModeIndex,
                optionsContent);

            return this.GUIPresets[newlySelectedIndex];
        }

        private void DrawPlaceholderSequenceField(Rect rect, int controlPrefix, string placeholderText)
        {
            EditorGUI.BeginDisabledGroup(true);
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, CustomSequenceContent.text));

            EditorGUI.TextField(rect, CustomSequenceContent, placeholderText);
            EditorGUI.EndDisabledGroup();
        }

        private string[] DrawCustomSequenceField(Rect rect, int controlPrefix, string[] displaySequence)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, CustomSequenceContent.text));

            var sequenceWithCommas = StringUtilities.AddCommasBetweenStrings(displaySequence);
            var customSequence = EditorGUI.TextField(rect, CustomSequenceContent, sequenceWithCommas);

            return StringUtilities.StripCommasFromString(customSequence);
        }

        private int DrawCountFromField(Rect rect, int controlPrefix, int originalIndex)
        {
            var weights = new float[] { 0.65f, 0.35f };
            var intFieldRect = rect.GetSplitHorizontalWeighted(1, 0.0f, weights);
            var content = new GUIContent(
                LocalizationManager.Instance.GetTranslation("countFrom"),
                LocalizationManager.Instance.GetTranslation("theValueToStartCounting"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, content.text));

            // Add and subtract 1 so that it displays as 1 based.
            var countFrom = EditorGUI.IntField(intFieldRect, content, originalIndex + 1) - 1;

            var stringToCountFrom = this.RenameOperation.GetStringFromSequenceForIndex(countFrom);
            if (!string.IsNullOrEmpty(stringToCountFrom))
            {
                EditorGUI.BeginDisabledGroup(true);
                var labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                var labelRect = rect.GetSplitHorizontalWeighted(2, 0.0f, weights);
                EditorGUI.LabelField(labelRect, new GUIContent(
                    LocalizationManager.Instance.GetTranslation("startsWith") + ": " + stringToCountFrom),
                    labelStyle);
                EditorGUI.EndDisabledGroup();
            }

            countFrom = Mathf.Max(countFrom, 0);
            return countFrom;
        }

        private int DrawIncrementField(Rect rect, int controlPrefix, int originalIncrement)
        {
            var content = new GUIContent(
                LocalizationManager.Instance.GetTranslation("increment"),
                LocalizationManager.Instance.GetTranslation("theValueToAddToTheCount"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, content.text));
            return EditorGUI.IntField(rect, content, originalIncrement);
        }

        private bool DrawPrependField(Rect rect, int controlPrefix, bool originalPrepend)
        {
            var content = new GUIContent(
                LocalizationManager.Instance.GetTranslation("addAsPrefix"),
                LocalizationManager.Instance.GetTranslation("addTheCountToTheFront"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, content.text));
            return EditorGUI.Toggle(rect, content, originalPrepend);
        }

        private void Initialize()
        {
            var uppercasePreset = new CountByLetterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("uppercaseAlphabet"),
                SequenceToDisplay = "A, B, C...",
                Preset = CountByLetterOperation.StringPreset.UppercaseAlphabet,
            };

            var lowercasePreset = new CountByLetterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("lowercaseAlphabet"),
                SequenceToDisplay = "a, b, c...",
                Preset = CountByLetterOperation.StringPreset.LowercaseAlphabet,
            };

            var customPreset = new CountByLetterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("custom"),
                CountSequence = new string[0],
                SequenceToDisplay = string.Empty,
                Preset = CountByLetterOperation.StringPreset.Custom,
            };

            this.GUIPresets = new List<CountByLetterPresetGUI>
            {
                uppercasePreset,
                lowercasePreset,
                customPreset
            };
        }

        private class CountByLetterPresetGUI
        {
            public string DisplayName { get; set; }

            public string[] CountSequence { get; set; }

            public string SequenceToDisplay { get; set; }

            public CountByLetterOperation.StringPreset Preset { get; set; }
        }
    }
}