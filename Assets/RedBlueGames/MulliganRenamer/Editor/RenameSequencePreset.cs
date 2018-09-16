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
            this.serializedOperationSequence = this.OperationSequence.ToSerializableString();
        }

        public void OnAfterDeserialize()
        {
            this.operationSequence = RenameOperationSequence<IRenameOperation>.FromString(
                this.serializedOperationSequence);
        }
    }
}