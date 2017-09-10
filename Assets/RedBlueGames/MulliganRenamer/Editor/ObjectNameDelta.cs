namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Object name delta tracks name changes to an Object.
    /// </summary>
    public class ObjectNameDelta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameDelta"/> class.
        /// </summary>
        /// <param name="obj">Object associated with these names.</param>
        /// <param name="newName">New name for the object</param>
        public ObjectNameDelta(UnityEngine.Object obj, string newName)
        {
            this.NamedObject = obj;
            this.OldName = obj.name;
            this.NewName = newName;
        }

        /// <summary>
        /// Gets the named object.
        /// </summary>
        /// <value>The named object.</value>
        public UnityEngine.Object NamedObject { get; }

        /// <summary>
        /// Gets the old name of the object.
        /// </summary>
        /// <value>The old name.</value>
        public string OldName { get; }

        /// <summary>
        /// Gets the new name of the object.
        /// </summary>
        /// <value>The new name.</value>
        public string NewName { get; }
    }
}
