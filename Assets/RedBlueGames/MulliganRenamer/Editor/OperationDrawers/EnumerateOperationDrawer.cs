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
                return "Add/Enumerate";
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
                return "Enumerate";
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
            var defaultHeight = this.CalculateGUIHeightForLines(4);
            var preferredHeight = defaultHeight;
            if (!this.Model.IsCountStringFormatValid)
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
            if (!this.Model.IsCountStringFormatValid)
            {
                weights = new float[] { 1, 1, 3, 1, 1 };
                countFormatWasValidBeforeDraw = false;
            }
            else
            {
                weights = new float[] { 1, 1, 1, 1 };
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
            this.Model.CountFormat = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFormatContent,
                selectedPreset.Format);
            EditorGUI.EndDisabledGroup();

            selectedPreset.Format = this.Model.CountFormat;

            if (!this.Model.IsCountStringFormatValid)
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
            this.Model.StartingCount = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFromContent,
                this.Model.StartingCount);

            var incrementContent = new GUIContent("Increment", "The value to add to each object when counting.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, incrementContent.text));
            this.Model.Increment = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                incrementContent,
                this.Model.Increment);
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