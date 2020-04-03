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

    public class EnumerateOperationDrawer : RenameOperationDrawer<EnumerateOperation>
    {
        public EnumerateOperationDrawer()
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
                return GetOperationPath("add", "count");
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
                return LocalizationManager.Instance.GetTranslation("count");
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

        private List<EnumeratePresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex
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
                        if (this.GUIPresets[i].Preset == this.RenameOperation.FormatPreset)
                        {
                            return i;
                        }
                    }

                    // Could not find a GUIPreset that uses the operation's preset.
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
            var defaultHeight = this.CalculateGUIHeightForLines(5);
            var preferredHeight = defaultHeight;
            if (!this.RenameOperation.IsCountStringFormatValid)
            {
                preferredHeight += this.GetHeightForHelpBox();
            }

            return preferredHeight;
        }

        private float GetHeightForHelpBox()
        {
            return 56.0f;
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var presetsContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("format"),
                LocalizationManager.Instance.GetTranslation("selectPresetFormat"));
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            var currentLine = 0;
            float[] weights;
            bool countFormatWasValidBeforeDraw;
            if (!this.RenameOperation.IsCountStringFormatValid)
            {
                weights = new float[] { 1, 1, 3, 1, 1, 1 };
                countFormatWasValidBeforeDraw = false;
            }
            else
            {
                weights = new float[] { 1, 1, 1, 1, 1 };
                countFormatWasValidBeforeDraw = true;
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            var newlySelectedIndex = EditorGUI.Popup(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                presetsContent,
                this.SelectedPresetIndex,
                names.ToArray());
            var selectedPreset = this.GUIPresets[newlySelectedIndex];

            EditorGUI.BeginDisabledGroup(selectedPreset.ReadOnly);
            var countFormatContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("countFormat"),
                LocalizationManager.Instance.GetTranslation("theStringFormatToUseWhenAddingTheCountToName"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFormatContent.text));
            if (selectedPreset.ReadOnly)
            {
                EditorGUI.TextField(operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                       countFormatContent,
                       this.RenameOperation.CountFormat);
                this.RenameOperation.SetCountFormatPreset(selectedPreset.Preset);
            }
            else
            {
                // Clear out the sequence when moving from a non-custom to Custom so that they don't
                // see it prepopulated with the previous preset's entries.
                if (this.RenameOperation.FormatPreset != EnumerateOperation.CountFormatPreset.Custom)
                {
                    this.RenameOperation.SetCountFormat("0");
                }

                this.RenameOperation.SetCountFormat(EditorGUI.TextField(
                    operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                    countFormatContent,
                    this.RenameOperation.CountFormat));
            }

            EditorGUI.EndDisabledGroup();

            if (!this.RenameOperation.IsCountStringFormatValid)
            {
                // On the first frame a user sets the count invalid, measurements will be broken because
                // the Height was calculated using the non-erroring size. So don't draw the error box until next frame
                if (countFormatWasValidBeforeDraw)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                var helpBoxMessage = LocalizationManager.Instance.GetTranslation("invalidCountFormat");
                var helpRect = operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights);
                helpRect = helpRect.AddPadding(4, 4, 4, 4);
                EditorGUI.HelpBox(helpRect, helpBoxMessage, MessageType.Warning);
            }

            var countFromContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("countFrom"),
                LocalizationManager.Instance.GetTranslation("theValueToStartCountingFrom"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFromContent.text));
            this.RenameOperation.StartingCount = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFromContent,
                this.RenameOperation.StartingCount);

            var incrementContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("increment"),
                LocalizationManager.Instance.GetTranslation("theValueToAddToEachObjectWhenCounting"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, incrementContent.text));
            this.RenameOperation.Increment = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                incrementContent,
                this.RenameOperation.Increment);

            var prependContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("addAsPrefix"),
                LocalizationManager.Instance.GetTranslation("addTheCountToTheFontOfTheObjectName"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, prependContent.text));
            this.RenameOperation.Prepend = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                prependContent,
                this.RenameOperation.Prepend);
        }

        private void Initialize()
        {
            var singleDigitPreset = new EnumeratePresetGUI()
            {
                DisplayName = "0, 1, 2...",
                Preset = EnumerateOperation.CountFormatPreset.SingleDigit,
                ReadOnly = true
            };

            var leadingZeroPreset = new EnumeratePresetGUI()
            {
                DisplayName = "00, 01, 02...",
                Preset = EnumerateOperation.CountFormatPreset.LeadingZero,
                ReadOnly = true
            };

            var underscorePreset = new EnumeratePresetGUI()
            {
                DisplayName = "_00, _01, _02...",
                Preset = EnumerateOperation.CountFormatPreset.Underscore,
                ReadOnly = true
            };

            var customPreset = new EnumeratePresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("custom"),
                Preset = EnumerateOperation.CountFormatPreset.Custom,
                ReadOnly = false
            };

            this.GUIPresets = new List<EnumeratePresetGUI>
            {
                singleDigitPreset,
                leadingZeroPreset,
                underscorePreset,
                customPreset
            };
        }

        private class EnumeratePresetGUI
        {
            public string DisplayName { get; set; }

            public EnumerateOperation.CountFormatPreset Preset { get; set; }

            public bool ReadOnly { get; set; }
        }
    }
}