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
                return GetOperationPath("delete", "removeCharacters");
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
                return LocalizationManager.Instance.GetTranslation("removeCharacters");
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
                return LocalizationManager.Instance.GetTranslation("preset");
            }
        }

        private List<CharacterPresetGUI> GUIPresets { get; set; }

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
                        if (this.GUIPresets[i].PresetID == this.RenameOperation.CurrentPresetID)
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
            var originalPresetIndex = this.SelectedPresetIndex;

            var currentSplit = 0;
            int numSplits = 2;
            if (this.GUIPresets[originalPresetIndex].IsReadOnly)
            {
                numSplits = 2;
            }
            else
            {
                numSplits = 3;
            }

            var presetsContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("preset"),
                LocalizationManager.Instance.GetTranslation("selectPresetOrSpecifyCharacters"));
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            var selectedPresetIndex = EditorGUI.Popup(
                operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                presetsContent,
                originalPresetIndex,
                names.ToArray());

            var selectedPreset = this.GUIPresets[selectedPresetIndex];
            var workingOptions = new RemoveCharactersOperation.RenameOptions();

            // We can't resize the Rects mid-GUI loop (GetHeight already said how tall it would be),
            // so if we've changed presets we just apply the defaults for the new change. They can
            // modify it next frame.
            if (selectedPresetIndex != originalPresetIndex)
            {
                if (selectedPreset.IsReadOnly)
                {
                    this.RenameOperation.SetOptionPreset(selectedPreset.PresetID);
                }
                else
                {
                    this.RenameOperation.SetOptions(workingOptions);
                }
                return;
            }

            if (selectedPreset.IsReadOnly)
            {
                // The Readonly Label just looks better disabled.
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
                var charactersFieldContent = new GUIContent(
                    LocalizationManager.Instance.GetTranslation("charactersToRemove"),
                    LocalizationManager.Instance.GetTranslation("allCharactersThatWillBeRemoved"));
                GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, charactersFieldContent.text));
                workingOptions.CharactersToRemove = EditorGUI.TextField(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    charactersFieldContent,
                    this.RenameOperation.CharactersToRemove);

                var caseSensitiveToggleContent = new GUIContent(
                    LocalizationManager.Instance.GetTranslation("caseSensitive"),
                    LocalizationManager.Instance.GetTranslation("flagTheSearchToMatchCase"));
                workingOptions.IsCaseSensitive = EditorGUI.Toggle(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    caseSensitiveToggleContent,
                    this.RenameOperation.IsCaseSensitive);
            }

            if (selectedPreset.IsReadOnly)
            {
                this.RenameOperation.SetOptionPreset(selectedPreset.PresetID);
            }
            else
            {
                this.RenameOperation.SetOptions(workingOptions);
            }
        }

        private void Initialize()
        {
            var symbolsPreset = new CharacterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("symbols"),
                ReadOnlyLabel = LocalizationManager.Instance.GetTranslation("removeSpecialCharacters"),
                PresetID = RemoveCharactersOperation.PresetID.Symbols,
                IsReadOnly = true
            };

            var numbersPreset = new CharacterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("numbers"),
                ReadOnlyLabel = LocalizationManager.Instance.GetTranslation("removeDigits"),
                PresetID = RemoveCharactersOperation.PresetID.Numbers,
                IsReadOnly = true
            };

            var whitespacePreset = new CharacterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("whitespace"),
                ReadOnlyLabel = LocalizationManager.Instance.GetTranslation("removesWhitespace"),
                PresetID = RemoveCharactersOperation.PresetID.Whitespace,
                IsReadOnly = true
            };

            var customPreset = new CharacterPresetGUI()
            {
                DisplayName = LocalizationManager.Instance.GetTranslation("custom"),
                PresetID = RemoveCharactersOperation.PresetID.Custom,
                IsReadOnly = false,
                ReadOnlyLabel = string.Empty
            };

            this.GUIPresets = new List<CharacterPresetGUI>
            {
                symbolsPreset,
                numbersPreset,
                whitespacePreset,
                customPreset
            };
        }

        private class CharacterPresetGUI
        {
            public string DisplayName { get; set; }

            public RemoveCharactersOperation.PresetID PresetID { get; set; }

            public string ReadOnlyLabel { get; set; }

            public bool IsReadOnly { get; set; }
        }
    }
}