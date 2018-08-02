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
                return "Add/Count";
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
                return "Count";
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

        private List<EnumeratePresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex { get; set; }

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
            var presetsContent = new GUIContent("Format", "Select a preset format or specify your own format.");
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
            this.SelectedPresetIndex = EditorGUI.Popup(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                presetsContent,
                this.SelectedPresetIndex,
                names.ToArray());
            var selectedPreset = this.GUIPresets[this.SelectedPresetIndex];

            EditorGUI.BeginDisabledGroup(selectedPreset.ReadOnly);
            var countFormatContent = new GUIContent("Count Format", "The string format to use when adding the Count to the name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFormatContent.text));
            this.RenameOperation.CountFormat = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFormatContent,
                selectedPreset.Format);
            EditorGUI.EndDisabledGroup();

            // Reapply the format back to the preset so that Custom gets updated. This prevents it from getting cleared out.
            selectedPreset.Format = this.RenameOperation.CountFormat;

            if (!this.RenameOperation.IsCountStringFormatValid)
            {
                // On the first frame a user sets the count invalid, measurements will be broken because
                // the Height was calculated using the non-erroring size. So don't draw the error box until next frame
                if (countFormatWasValidBeforeDraw)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                var helpBoxMessage = "Invalid Count Format. Typical formats are D1 for one digit with no " +
                                     "leading zeros, D2, for two, etc." +
                                     "\nLookup the String.Format() method for more info on formatting options.";
                var helpRect = operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights);
                helpRect = helpRect.AddPadding(4, 4, 4, 4);
                EditorGUI.HelpBox(helpRect, helpBoxMessage, MessageType.Warning);
            }

            var countFromContent = new GUIContent("Count From", "The value to start counting from. The first object will have this number.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFromContent.text));
            this.RenameOperation.StartingCount = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFromContent,
                this.RenameOperation.StartingCount);

            var incrementContent = new GUIContent("Increment", "The value to add to each object when counting.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, incrementContent.text));
            this.RenameOperation.Increment = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                incrementContent,
                this.RenameOperation.Increment);

            var prependContent = new GUIContent("Add as Prefix", "Add the count to the front of the object's name.");
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
                Format = "0",
                ReadOnly = true
            };

            var leadingZeroPreset = new EnumeratePresetGUI()
            {
                DisplayName = "00, 01, 02...",
                Format = "00",
                ReadOnly = true
            };

            var underscorePreset = new EnumeratePresetGUI()
            {
                DisplayName = "_00, _01, _02...",
                Format = "_00",
                ReadOnly = true
            };

            var customPreset = new EnumeratePresetGUI()
            {
                DisplayName = "Custom",
                Format = string.Empty,
                ReadOnly = false
            };

            this.GUIPresets = new List<EnumeratePresetGUI>
            {
                singleDigitPreset,
                leadingZeroPreset,
                underscorePreset,
                customPreset
            };

            this.SelectedPresetIndex = 0;
        }

        private class EnumeratePresetGUI
        {
            public string DisplayName { get; set; }

            public string Format { get; set; }

            public bool ReadOnly { get; set; }
        }
    }
}