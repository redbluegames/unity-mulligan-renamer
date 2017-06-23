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

namespace RedBlueGames.BulkRename
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class RemoveCharactersOperation : BaseRenameOperation
    {
        public static readonly RemoveCharactersOperationOptions Symbols = new RemoveCharactersOperationOptions()
        {
            CharactersToRemove = "\\W",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };
        
        public static readonly RemoveCharactersOperationOptions Numbers = new RemoveCharactersOperationOptions()
        {
            CharactersToRemove = "\\d",
            CharactersAreRegex = true,
            IsCaseSensitive = false
        };
        
        private RemoveCharactersOperationOptions custom = new RemoveCharactersOperationOptions()
        {
            CharactersToRemove = string.Empty,
            IsCaseSensitive = false,
            CharactersAreRegex = false
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.RemoveCharactersOperation"/> class.
        /// </summary>
        public RemoveCharactersOperation()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.RemoveCharactersOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public RemoveCharactersOperation(RemoveCharactersOperation operationToCopy)
        {
            this.Initialize();
            this.Options = operationToCopy.Options;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Remove Characters";
            }
        }

        /// <summary>
        /// Gets the order in which this rename op is displayed in the Add Op menu (lower is higher in the list.)
        /// </summary>
        /// <value>The menu order.</value>
        public override int MenuOrder
        {
            get
            {
                return 6;
            }
        }

        /// <summary> 
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Remove Characters";
            }
        }

        /// <summary>
        /// Gets or sets the options used to configure the Rename Operation.
        /// </summary>
        /// <value>The options.</value>
        public RemoveCharactersOperationOptions Options { get; set; }

        private List<CharacterPresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex { get; set; }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new RemoveCharactersOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override string Rename(string input, int relativeCount)
        {
            if (!string.IsNullOrEmpty(this.Options.CharactersToRemove))
            {
                var regexOptions = this.Options.IsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                var replacement = string.Empty;

                try
                {
                    var regexPattern = this.Options.CharactersToRemove;
                    if (!this.Options.CharactersAreRegex)
                    {
                        regexPattern = Regex.Escape(regexPattern);
                    }

                    var charactersAsRegex = string.Concat("[", regexPattern, "]");
                    return Regex.Replace(input, charactersAsRegex, replacement, regexOptions);
                }
                catch (System.ArgumentException e)
                {
                    throw new System.ArgumentException(
                        string.Format(
                            "Trying to Rename a string by RemovingCharacters using an invalid RegEx expression [{0}]." +
                            " Please supply valid RegEx or unflag the operation as Regex.",
                            this.Options.CharactersToRemove),
                        e);
                }
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected override void DrawContents()
        {
            var presetsContent = new GUIContent("Preset", "Select a preset or specify your own characters with Custom.");
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            this.SelectedPresetIndex = EditorGUILayout.Popup(presetsContent, this.SelectedPresetIndex, names.ToArray());
            var selectedPreset = this.GUIPresets[this.SelectedPresetIndex];

            var workingOptions = selectedPreset.Options;
            EditorGUI.BeginDisabledGroup(selectedPreset.IsReadOnly);

            if (selectedPreset.IsReadOnly)
            {
                var readonlyLabelContent = new GUIContent(selectedPreset.ReadOnlyLabel);
                var labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleRight;
                EditorGUILayout.LabelField(readonlyLabelContent, labelStyle);
            }
            else
            {
                var charactersFieldContent = new GUIContent("Characters to Remove", "All characters that will be removed from the names.");
                workingOptions.CharactersToRemove = EditorGUILayout.TextField(charactersFieldContent, workingOptions.CharactersToRemove);

                var caseSensitiveToggleContent = new GUIContent("Case Sensitive", "Flag the search to match only the specified case");
                workingOptions.IsCaseSensitive = EditorGUILayout.Toggle(caseSensitiveToggleContent, workingOptions.IsCaseSensitive);
            }

            EditorGUI.EndDisabledGroup();

            // Structs were copied by value, so reapply the modified structs back to their sources
            this.Options = workingOptions;
            selectedPreset.Options = workingOptions;
        }

        private void Initialize()
        {
            var symbolsPreset = new CharacterPresetGUI()
            {
                DisplayName = "Symbols",
                ReadOnlyLabel = "Removes special characters (ie. !@#$%^&*)",
                Options = Symbols,
                IsReadOnly = true
            };

            var numbersPreset = new CharacterPresetGUI()
            {
                DisplayName = "Numbers",
                ReadOnlyLabel = "Removes digits 0-9",
                Options = Numbers,
                IsReadOnly = true
            };

            var customPreset = new CharacterPresetGUI()
            {
                DisplayName = "Custom",
                Options = this.custom
            };

            this.GUIPresets = new List<CharacterPresetGUI>
            {
                symbolsPreset,
                numbersPreset,
                customPreset
            };

            this.SelectedPresetIndex = 0;
        }

        /// <summary>
        /// Options used to configure RemoveCharactersOperations.
        /// </summary>
        public struct RemoveCharactersOperationOptions
        {
            /// <summary>
            /// Gets or sets the characters to remove.
            /// </summary>
            /// <value>The characters to remove.</value>
            public string CharactersToRemove { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this
            /// <see cref="RedBlueGames.BulkRename.RemoveCharactersOperation+RemoveCharactersOperationOptions"/>
            /// characters are regex symbols.
            /// </summary>
            /// <value><c>true</c> if characters are regex; otherwise, <c>false</c>.</value>
            public bool CharactersAreRegex { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the characters are matched using case sensitivity.
            /// </summary>
            /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
            public bool IsCaseSensitive { get; set; }
        }

        private class CharacterPresetGUI
        {
            public string DisplayName { get; set; }

            public RemoveCharactersOperationOptions Options { get; set; }

            public string ReadOnlyLabel { get; set; }

            public bool IsReadOnly { get; set; }
        }
    }
}