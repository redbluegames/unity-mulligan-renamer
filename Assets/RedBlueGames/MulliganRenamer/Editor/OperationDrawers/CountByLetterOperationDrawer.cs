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
                return "Add/Count By Letter";
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
                return "Count by Letter";
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
                return "Format";
            }
        }

        private List<CountByLetterPresetGUI> GUIPresets { get; set; }

        private int SelectedModeIndex { get; set; }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(4);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var currentRectSplit = 0;
            var numLines = 4;

            var sequenceRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            var newlySelectedPreset = this.DrawSequenceSelection(sequenceRect, controlPrefix);

            var customSequenceRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            this.RenameOperation.CountSequence = this.DrawCustomSequenceField(
                customSequenceRect,
                controlPrefix,
                newlySelectedPreset);

            // Reapply the count sequence to the preset so that it doesn't get cleared out when they click away.
            // This keeps it from being pre-populated with the alphabet when you switch to Custom from Alphabet.
            newlySelectedPreset.CountSequence = this.RenameOperation.CountSequence;

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
        }

        private CountByLetterPresetGUI DrawSequenceSelection(Rect rect, int controlPrefix)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Format"));
            var modeContent = new GUIContent("Format", "Format for the added letters.");
            var optionsContent = new GUIContent[this.GUIPresets.Count];
            for (int i = 0; i < optionsContent.Length; ++i)
            {
                optionsContent[i] = new GUIContent(this.GUIPresets[i].DisplayName);
            }

            this.SelectedModeIndex = EditorGUI.Popup(
                rect,
                modeContent,
                this.SelectedModeIndex,
                optionsContent);

            return this.GUIPresets[this.SelectedModeIndex];
        }

        private string[] DrawCustomSequenceField(Rect rect, int controlPrefix, CountByLetterPresetGUI preset)
        {
            EditorGUI.BeginDisabledGroup(preset.IsReadOnly);
            var customSequenceContent = new GUIContent("Strings", "The string format to use when adding the Count to the name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, customSequenceContent.text));
            var displaySequence = preset.IsReadOnly ? preset.DisplaySequence :
                                        StringUtilities.AddCommasBetweenStrings(preset.CountSequence);
            var customSequence = EditorGUI.TextField(rect, customSequenceContent, displaySequence);
            EditorGUI.EndDisabledGroup();

            if (preset.IsReadOnly)
            {
                return preset.CountSequence;
            }
            else
            {
                return StringUtilities.StripCommasFromString(customSequence);
            }
        }

        private int DrawCountFromField(Rect rect, int controlPrefix, int originalIndex)
        {
            var weights = new float[] { 0.65f, 0.35f };
            var intFieldRect = rect.GetSplitHorizontalWeighted(1, 0.0f, weights);
            var content = new GUIContent("Count From", "The value to start counting from. " +
                                         "The string from the sequence at this count will be appended to the first object.");
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
                EditorGUI.LabelField(labelRect, new GUIContent("Starts with: " + stringToCountFrom), labelStyle);
                EditorGUI.EndDisabledGroup();
            }

            countFrom = Mathf.Max(countFrom, 0);
            return countFrom;
        }

        private int DrawIncrementField(Rect rect, int controlPrefix, int originalIncrement)
        {
            var content = new GUIContent("Increment", "The value to add to the count after naming an object.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, content.text));
            return EditorGUI.IntField(rect, content, originalIncrement);
        }

        private void Initialize()
        {
            var uppercasePreset = new CountByLetterPresetGUI()
            {
                DisplayName = "Uppercase Alphabet",
                CountSequence = CountByLetterOperation.UppercaseAlphabet,
                DisplaySequence = "A,B,C...",
                IsReadOnly = true
            };

            var lowercasePreset = new CountByLetterPresetGUI()
            {
                DisplayName = "Lowercase Alphabet",
                CountSequence = CountByLetterOperation.LowercaseAlphabet,
                DisplaySequence = "a,b,c...",
                IsReadOnly = true
            };

            var customPreset = new CountByLetterPresetGUI()
            {
                DisplayName = "Custom",
                CountSequence = new string[0],
                DisplaySequence = "",
                IsReadOnly = false
            };

            this.GUIPresets = new List<CountByLetterPresetGUI>
            {
                uppercasePreset,
                lowercasePreset,
                customPreset
            };

            this.SelectedModeIndex = 0;
        }

        private class CountByLetterPresetGUI
        {
            public string DisplayName { get; set; }

            public string[] CountSequence { get; set; }

            public string DisplaySequence { get; set; }

            public bool IsReadOnly { get; set; }
        }
    }
}