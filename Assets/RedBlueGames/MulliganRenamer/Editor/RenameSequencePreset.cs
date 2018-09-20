namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RenameSequencePreset : UnityEngine.ISerializationCallbackReceiver
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string serializedOperationSequence;

        private RenameOperationSequence<IRenameOperation> operationSequence;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public RenameOperationSequence<IRenameOperation> OperationSequence
        {
            get
            {
                return this.operationSequence;
            }

            set
            {
                this.operationSequence = value;
            }
        }

        public void OnBeforeSerialize()
        {
            if (this.OperationSequence != null)
            {
                this.serializedOperationSequence = this.OperationSequence.ToSerializableString();
            }
            else
            {
                this.serializedOperationSequence = string.Empty;
            }
        }

        public void OnAfterDeserialize()
        {
            this.operationSequence = RenameOperationSequence<IRenameOperation>.FromString(
                this.serializedOperationSequence);
        }

        public override int GetHashCode()
        {
            // I'm never going to hash these so just use base
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherPreset = obj as RenameSequencePreset;
            if (otherPreset == null)
            {
                return false;
            }

            if (this.Name != otherPreset.Name)
            {
                return false;
            }

            return this.OperationSequence.Equals(otherPreset.OperationSequence);
        }
    }
}