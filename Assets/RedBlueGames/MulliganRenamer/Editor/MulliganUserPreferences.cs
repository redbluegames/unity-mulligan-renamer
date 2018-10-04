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

    /// <summary>
    /// Maintains serializable information about a user's session in Mulligan.
    /// </summary>
    [System.Serializable]
    public class MulliganUserPreferences : ISerializationCallbackReceiver
    {
        private const int NumberOfSessionsBeforeReviewPrompt = 3;

        [SerializeField]
        private string lastUsedPresetName;

        [SerializeField]
        private string serializedPreviousSequence;

        [SerializeField]
        private RenameOperationSequence<IRenameOperation> previousSequence;

        [SerializeField]
        private List<RenameSequencePreset> savedPresets;

        [SerializeField]
        private int numSessionsUsed;

        [SerializeField]
        private bool hasClickedPrompt;

        [SerializeField]
        private bool hasShownThanks;

        /// <summary>
        /// Gets or Sets the previously used Sequence of Rename Operations
        /// </summary>
        public RenameOperationSequence<IRenameOperation> PreviousSequence
        {
            get
            {
                return this.previousSequence;
            }

            set
            {
                this.previousSequence = value;
            }
        }

        /// <summary>
        /// Gets or Sets a list of saved RenameSequencePresets
        /// </summary>
        public List<RenameSequencePreset> SavedPresets
        {
            get
            {
                return this.savedPresets;
            }

            set
            {
                this.savedPresets = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the most recently used preset
        /// </summary>
        public string LastUsedPresetName
        {
            get
            {
                return this.lastUsedPresetName;
            }

            set
            {
                this.lastUsedPresetName = value;
            }
        }

        /// <summary>
        /// Gets a list of all saved preset names
        /// </summary>
        public List<string> PresetNames
        {
            get
            {
                var names = new List<string>(this.savedPresets.Count);
                foreach (var preset in this.savedPresets)
                {
                    names.Add(preset.Name);
                }

                return names;
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating the number of times the user has used the rename tool.
        /// The intent is for a session to be every time the tool is opened.
        /// </summary>
        public int NumSessionsUsed
        {
            get
            {
                return this.numSessionsUsed;
            }

            set
            {
                this.numSessionsUsed = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the preferences suggest the user should be prompted to leave a review
        /// </summary>
        public bool HasConfirmedReviewPrompt
        {
            get
            {
                return this.hasClickedPrompt;
            }

            set
            {
                this.hasClickedPrompt = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the preferences suggest the user should be prompted to leave a review
        /// </summary>
        public bool NeedsReview
        {
            get
            {
                var hasBeenUsedEnough = this.NumSessionsUsed >= NumberOfSessionsBeforeReviewPrompt;
                return hasBeenUsedEnough && !this.HasConfirmedReviewPrompt;
            }
        }

        /// <summary>
        /// Create a new Instance of MulliganUserPreferences
        /// </summary>
        public MulliganUserPreferences()
        {
            // Default previous sequence to a replace string op just because it's
            // most user friendly
            this.previousSequence = new RenameOperationSequence<IRenameOperation>();
            this.previousSequence.Add(new ReplaceStringOperation());

            this.savedPresets = new List<RenameSequencePreset>();
        }

        /// <summary>
        /// Get a saved preset by name, or null if none exists
        /// </summary>
        /// <param name="name">Name of the preset to find</param>
        /// <returns>The preset with the specified name, if it exists; null otherwise</returns>
        public RenameSequencePreset FindSavedPresetWithName(string name)
        {
            var index = this.FindIndexOfSavedPresetWithName(name);
            if (index >= 0)
            {
                return this.SavedPresets[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the index of the saved preset that shares the specified name, or -1
        /// </summary>
        /// <param name="name">Name of the preset to find</param>
        /// <returns>The index of the preset that matches the name, or else -1</returns>
        public int FindIndexOfSavedPresetWithName(string name)
        {
            for (int i = 0; i < this.SavedPresets.Count; ++i)
            {
                if (this.SavedPresets[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Save the specified RenameSequencePreset into the user's list of saved presets
        /// </summary>
        /// <param name="preset">Preset to save</param>
        public void SavePreset(RenameSequencePreset preset)
        {
            var existingPresetIndex = this.FindIndexOfSavedPresetWithName(preset.Name);

            if (existingPresetIndex >= 0)
            {
                this.SavedPresets.RemoveAt(existingPresetIndex);
                this.SavedPresets.Insert(existingPresetIndex, preset);
            }
            else
            {
                this.SavedPresets.Add(preset);
            }
        }

        /// <summary>
        /// Unity's callback before serializing the object
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.serializedPreviousSequence = this.PreviousSequence.ToSerializableString();
        }

        /// <summary>
        /// Unity's callback after deserializing the object
        /// </summary>
        public void OnAfterDeserialize()
        {
            this.PreviousSequence = RenameOperationSequence<IRenameOperation>.FromString(
                this.serializedPreviousSequence);
        }
    }
}
