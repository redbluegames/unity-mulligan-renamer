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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation that enumerates characters onto the end of the rename string.
    /// </summary>
    public class EnumerateOperation : RenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerateOperation"/> class.
        /// </summary>
        public EnumerateOperation()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerateOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public EnumerateOperation(EnumerateOperation operationToCopy)
        {
            this.StartingCount = operationToCopy.StartingCount;
            this.CountFormat = operationToCopy.CountFormat;
            this.Increment = operationToCopy.Increment;

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
        /// Gets or sets the starting count.
        /// </summary>
        /// <value>The starting count.</value>
        public int StartingCount { get; set; }

        /// <summary>
        /// Gets or sets the format for the count, appended to the end of the string.
        /// </summary>
        /// <value>The count format.</value>
        public string CountFormat { get; set; }

        /// <summary>
        /// Gets or sets the increment to use when counting.
        /// </summary>
        public int Increment { get; set; }

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
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override RenameOperation Clone()
        {
            var clone = new EnumerateOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override RenameResult Rename(string input, int relativeCount)
        {
            var renameResult = new RenameResult();
            if (!string.IsNullOrEmpty(input))
            {
                renameResult.Add(new Diff(input, DiffOperation.Equal));
            }

            if (!string.IsNullOrEmpty(this.CountFormat))
            {
                var currentCount = this.StartingCount + (relativeCount * this.Increment);
                try
                {
                    var currentCountAsString = currentCount.ToString(this.CountFormat);
                    renameResult.Add(new Diff(currentCountAsString, DiffOperation.Insertion));
                }
                catch (System.FormatException)
                {
                    // Can't append anything if format is bad.
                }
            }

            return renameResult;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(int controlPrefix)
        {   
            var presetsContent = new GUIContent("Format", "Select a preset format or specify your own format.");
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            this.SelectedPresetIndex = EditorGUILayout.Popup(presetsContent, this.SelectedPresetIndex, names.ToArray());
            var selectedPreset = this.GUIPresets[this.SelectedPresetIndex];

            EditorGUI.BeginDisabledGroup(selectedPreset.ReadOnly);
            var countFormatContent = new GUIContent("Count Format", "The string format to use when adding the Count to the name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFormatContent.text));
            this.CountFormat = EditorGUILayout.TextField(countFormatContent, selectedPreset.Format);
            EditorGUI.EndDisabledGroup();

            try
            {
                this.StartingCount.ToString(this.CountFormat);
            }
            catch (System.FormatException)
            {
                var helpBoxMessage = "Invalid Count Format. Typical formats are D1 for one digit with no " +
                                     "leading zeros, D2, for two, etc." +
                                     "\nLookup the String.Format() method for more info on formatting options.";
                EditorGUILayout.HelpBox(helpBoxMessage, MessageType.Warning);
            }

            var countFromContent = new GUIContent("Count From", "The value to start counting from. The first object will have this number.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFromContent.text));
            this.StartingCount = EditorGUILayout.IntField(countFromContent, this.StartingCount);

            var incrementContent = new GUIContent("Increment", "The value to add to each object when counting.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, incrementContent.text));
            this.Increment = EditorGUILayout.IntField(incrementContent, this.Increment);

            selectedPreset.Format = this.CountFormat;
        }

        private void Initialize()
        {
            this.Increment = 1;

            // Give it an initially valid count format
            this.CountFormat = "0";
                
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