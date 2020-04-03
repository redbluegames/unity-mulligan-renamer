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
        public UnityEngine.Object NamedObject { get; private set; }

        /// <summary>
        /// Gets the old name of the object.
        /// </summary>
        /// <value>The old name.</value>
        public string OldName { get; private set; }

        /// <summary>
        /// Gets the new name of the object.
        /// </summary>
        /// <value>The new name.</value>
        public string NewName { get; private set; }
    }
}
