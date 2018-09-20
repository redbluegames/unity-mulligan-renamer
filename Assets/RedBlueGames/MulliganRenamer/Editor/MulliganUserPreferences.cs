namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class MulliganUserPreferences : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string lastUsedPresetName;

        [SerializeField]
        private string serializedPreviousSequence;

        [SerializeField]
        private RenameOperationSequence<IRenameOperation> previousSequence;

        [SerializeField]
        private List<RenameSequencePreset> savedPresets;

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

        public MulliganUserPreferences()
        {
            // Default previous sequence to a replace string op just because it's
            // most user friendly
            this.previousSequence = new RenameOperationSequence<IRenameOperation>();
            this.previousSequence.Add(new ReplaceStringOperation());

            this.savedPresets = new List<RenameSequencePreset>();
        }

        public RenameSequencePreset GetSavedPresetWithName(string name)
        {
            var index = this.GetSavedPresetIndexWithName(name);
            if (index >= 0)
            {
                return this.SavedPresets[index];
            }
            else
            {
                return null;
            }
        }

        public int GetSavedPresetIndexWithName(string name)
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

        public void SavePreset(RenameSequencePreset preset)
        {
            var existingPresetIndex = this.GetSavedPresetIndexWithName(preset.Name);

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

        public void OnBeforeSerialize()
        {
            this.serializedPreviousSequence = this.PreviousSequence.ToSerializableString();
        }

        public void OnAfterDeserialize()
        {
            this.PreviousSequence = RenameOperationSequence<IRenameOperation>.FromString(
                this.serializedPreviousSequence);
        }
    }
}
