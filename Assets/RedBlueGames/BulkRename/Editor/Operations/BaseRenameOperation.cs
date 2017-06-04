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
    /// The base RenameOperation used by BulkRenamer. All Operations derive from this,
    /// allowing for shared code.
    /// </summary>
    public abstract class BaseRenameOperation : IRenameOperation
    {
        private const string AddedTextColorTag = "<color=green>";
        private const string DeletedTextColorTag = "<color=red>";
        private const string EndColorTag = "</color>";

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public abstract string MenuDisplayPath { get; }

        /// <summary>
        /// Gets the order in which this rename op is displayed in the Add Op menu (lower is higher in the list.)
        /// </summary>
        /// <value>The menu order.</value>
        public abstract int MenuOrder { get; }

        /// <summary>
        /// Rename the specified input, using the relativeCount. Optionally output the string as a diff.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <param name="includeDiff">If set to <c>true</c> include diff.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public abstract string Rename(string input, int relativeCount, bool includeDiff);

        /// <summary>
        /// Draws the element as a GUI using EditorGUILayout calls. This should return a copy of the 
        /// Operation with the modified data. This way we mirror how regular GUI calls work.
        /// </summary>
        /// <returns>A modified copy of the Operation.</returns>
        public abstract BaseRenameOperation DrawGUI();

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public abstract BaseRenameOperation Clone();

        /// <summary>
        /// Colors a string to represent an Added string (for a diff)
        /// </summary>
        /// <returns>The string colored as if it was added.</returns>
        /// <param name="stringToColor">String to color.</param>
        protected static string ColorStringForAdd(string stringToColor)
        {
            return string.Concat(AddedTextColorTag, stringToColor, EndColorTag);
        }

        /// <summary>
        /// Colors a string to represent a Deleted string (for a diff)
        /// </summary>
        /// <returns>The string colored as if it was deleted.</returns>
        /// <param name="stringToColor">String to color.</param>
        protected static string ColorStringForDelete(string stringToColor)
        {
            return string.Concat(DeletedTextColorTag, stringToColor, EndColorTag);
        }
    }
}