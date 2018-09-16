namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class MulliganUserPreferences : ISerializationCallbackReceiver
    {
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

        public MulliganUserPreferences()
        {
            // Default previous sequence to a replace string op just because it's
            // most user friendly
            this.previousSequence = new RenameOperationSequence<IRenameOperation>();
            this.previousSequence.Add(new ReplaceStringOperation());

            this.savedPresets = new List<RenameSequencePreset>();
        }

        public void OnBeforeSerialize()
        {
            this.serializedPreviousSequence = this.PreviousSequence.ToSerializableString();

            // TODO: DO WE NEED TO SERIALIZE THIS LIKE THIS?
            /*
            foreach (var preset in this.ActivePreferences.SavedPresets)
            {
                stringBuilder.Append("Preset:");
                var presetAsJson = JsonUtility.ToJson(preset);
                Debug.Log(presetAsJson);
                stringBuilder.Append(JsonUtility.ToJson(preset));
                stringBuilder.Append("}\n");
            }
			 */
        }

        public void OnAfterDeserialize()
        {
            this.PreviousSequence = RenameOperationSequence<IRenameOperation>.FromString(
                this.serializedPreviousSequence);
        }
    }
}
