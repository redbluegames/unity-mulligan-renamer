namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class RemoveCharactersOperationDrawer : RenameOperationDrawer<RemoveCharactersOperation>
    {
        public RemoveCharactersOperationDrawer()
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
                return "Delete/Remove Characters";
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
                return "Remove Characters";
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
                return this.DeleteColor;
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
                return "Preset";
            }
        }

        private List<CharacterPresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex { get; set; }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            var selectedPreset = this.GUIPresets[this.SelectedPresetIndex];
            int numGUILines;
            if (selectedPreset.IsReadOnly)
            {
                numGUILines = 2;
            }
            else
            {
                numGUILines = 3;
            }

            return this.CalculateGUIHeightForLines(numGUILines);
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            // Read and write into copies so that we don't resize the view while it's being worked on,
            // which is what is required when the user switches settings around and options (lines) are added into the GUI,
            // after it's already been measured based on it's PRE Update state.
            var selectedPresetIndexPreDraw = this.SelectedPresetIndex;
            var selectedPresetIndexPostDraw = selectedPresetIndexPreDraw;
            var modelPreDraw = (RemoveCharactersOperation)this.Model.Clone();
            var modelPostDraw = (RemoveCharactersOperation)modelPreDraw.Clone();

            var presetsContent = new GUIContent("Preset", "Select a preset or specify your own characters.");
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            var currentSplit = 0;
            int numSplits = 2;
            if (this.GUIPresets[selectedPresetIndexPreDraw].IsReadOnly)
            {
                numSplits = 2;
            }
            else
            {
                numSplits = 3;
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            selectedPresetIndexPostDraw = EditorGUI.Popup(
                operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                presetsContent,
                selectedPresetIndexPreDraw,
                names.ToArray());

            var selectedPreset = this.GUIPresets[selectedPresetIndexPreDraw];
            var workingConfig = selectedPreset.Options;

            if (selectedPreset.IsReadOnly)
            {
                // Label just looks better disabled.
                EditorGUI.BeginDisabledGroup(true);
                var readonlyLabelContent = new GUIContent(selectedPreset.ReadOnlyLabel);
                var labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleRight;
                EditorGUI.LabelField(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    readonlyLabelContent,
                    labelStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                var charactersFieldContent = new GUIContent("Characters to Remove", "All characters that will be removed from the names.");
                GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, charactersFieldContent.text));
                workingConfig.CharactersToRemove = EditorGUI.TextField(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    charactersFieldContent,
                    workingConfig.CharactersToRemove);

                var caseSensitiveToggleContent = new GUIContent("Case Sensitive", "Flag the search to match only the specified case");
                workingConfig.IsCaseSensitive = EditorGUI.Toggle(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    caseSensitiveToggleContent,
                    workingConfig.IsCaseSensitive);
            }

            // Structs were copied by value, so reapply the modified structs back to their sources
            modelPostDraw.Config = workingConfig;

            // Apply model back to this version to be represented next frame.
            this.Model.CopyFrom(modelPostDraw);

            // Also apply working gui state into this object so that it's represented next frame
            this.GUIPresets[selectedPresetIndexPreDraw].Options = workingConfig;
            this.SelectedPresetIndex = selectedPresetIndexPostDraw;
        }

        private void Initialize()
        {
            var symbolsPreset = new CharacterPresetGUI()
            {
                DisplayName = "Symbols",
                ReadOnlyLabel = "Removes special characters (ie. !@#$%^&*)",
                Options = RemoveCharactersOperation.Symbols,
                IsReadOnly = true
            };

            var numbersPreset = new CharacterPresetGUI()
            {
                DisplayName = "Numbers",
                ReadOnlyLabel = "Removes digits 0-9",
                Options = RemoveCharactersOperation.Numbers,
                IsReadOnly = true
            };

            var customOptions = new RemoveCharactersOperation.Configuration();
            customOptions.CharactersToRemove = string.Empty;
            customOptions.IsCaseSensitive = false;
            customOptions.CharactersAreRegex = false;
            var customPreset = new CharacterPresetGUI()
            {
                DisplayName = "Custom",
                Options = customOptions,
                IsReadOnly = false,
                ReadOnlyLabel = string.Empty
            };

            this.GUIPresets = new List<CharacterPresetGUI>
            {
                symbolsPreset,
                numbersPreset,
                customPreset
            };
        }

        private class CharacterPresetGUI
        {
            public string DisplayName { get; set; }

            public RemoveCharactersOperation.Configuration Options { get; set; }

            public string ReadOnlyLabel { get; set; }

            public bool IsReadOnly { get; set; }

            public CharacterPresetGUI()
            {
                this.DisplayName = string.Empty;
                this.Options = new RemoveCharactersOperation.Configuration();
                this.ReadOnlyLabel = string.Empty;
                this.IsReadOnly = false;
            }

            public CharacterPresetGUI(CharacterPresetGUI other)
            {
                this.DisplayName = other.DisplayName;
                this.Options = other.Options;
                this.ReadOnlyLabel = other.ReadOnlyLabel;
                this.IsReadOnly = other.IsReadOnly;
            }
        }
    }
}