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
        /// <summary>
        /// Events that are returned by the GUI draw call to indicate what input was pressed.
        /// </summary>
        public enum ListButtonEvent
        {
            /// <summary>
            /// No button was clicked.
            /// </summary>
            None,

            /// <summary>
            /// The move up button was clicked.
            /// </summary>
            MoveUp,

            /// <summary>
            /// The move down button was clicked.
            /// </summary>
            MoveDown,

            /// <summary>
            /// The delete button was clicked.
            /// </summary>
            Delete
        }

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
        /// Gets a value indicating whether this instance has errors that prevent it from Renaming.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public virtual bool HasErrors
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected abstract string HeadingLabel { get; }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public abstract string Rename(string input, int relativeCount);

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public abstract BaseRenameOperation Clone();

        /// <summary>
        /// Draws the element as a GUI using EditorGUILayout calls. This should return a copy of the 
        /// Operation with the modified data. This way we mirror how regular GUI calls work.
        /// </summary>
        /// <param name = "disableUpButton">Draw the GUI with a disabled MoveUp button.</param>
        /// <param name = "disableDownButton">Draw the GUI with a disabled MoveDown button.</param>
        /// <returns>A ListButtonEvent indicating if a button was clicked.</returns>
        public ListButtonEvent DrawGUI(bool disableUpButton, bool disableDownButton)
        {
            var operationStyle = new GUIStyle(GUI.skin.FindStyle("ScriptText"));
            operationStyle.stretchHeight = false;
            operationStyle.padding = new RectOffset(6, 6, 4, 4);
            EditorGUILayout.BeginVertical(operationStyle);
            ListButtonEvent buttonEvent = this.DrawHeaderAndReorderButtons(
                                              this.HeadingLabel,
                                              disableUpButton,
                                              disableDownButton);
            EditorGUI.indentLevel++;
            this.DrawContents();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            return buttonEvent;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected abstract void DrawContents();

        /// <summary>
        /// Draws the header and reorder buttons.
        /// </summary>
        /// <returns>The header and reorder buttons.</returns>
        /// <param name="header">Header text to display.</param>
        /// <param name="disableUpButton">If set to <c>true</c> disable the MoveUp button.</param>
        /// <param name="disableDownButton">If set to <c>true</c> disable the MoveDown button.</param>
        protected ListButtonEvent DrawHeaderAndReorderButtons(string header, bool disableUpButton, bool disableDownButton)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            ListButtonEvent buttonEvent = ListButtonEvent.None;
            buttonEvent = this.DrawReorderingButtons(disableUpButton, disableDownButton);

            EditorGUILayout.EndHorizontal();

            return buttonEvent;
        }

        private ListButtonEvent DrawReorderingButtons(bool disableUpButton, bool disableDownButton)
        {
            const string BigUpArrowUnicode = "\u25B2";
            const string BigDownArrowUnicode = "\u25BC";
            ListButtonEvent buttonEvent = ListButtonEvent.None;

            EditorGUI.BeginDisabledGroup(disableUpButton);

            var leftButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight - 2;
            leftButtonStyle.fixedWidth = 20;
            leftButtonStyle.margin = new RectOffset(-1, -1, leftButtonStyle.margin.top, leftButtonStyle.margin.bottom);
            leftButtonStyle.fontSize = 8;
            if (GUILayout.Button(BigUpArrowUnicode, leftButtonStyle))
            {
                buttonEvent = ListButtonEvent.MoveUp;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(disableDownButton);

            var midButtonStyle = new GUIStyle(EditorStyles.miniButtonMid);
            midButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight - 2;
            midButtonStyle.fixedWidth = 20;
            midButtonStyle.margin = new RectOffset(-1, -1, midButtonStyle.margin.top, midButtonStyle.margin.bottom);
            midButtonStyle.fontSize = 8;
            if (GUILayout.Button(BigDownArrowUnicode, midButtonStyle))
            {
                buttonEvent = ListButtonEvent.MoveDown;
            }

            EditorGUI.EndDisabledGroup();

            var rightButtonStyle = new GUIStyle(EditorStyles.miniButtonRight);
            rightButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight - 2;
            rightButtonStyle.fixedWidth = 20;
            rightButtonStyle.margin = new RectOffset(-1, -1, rightButtonStyle.margin.top, rightButtonStyle.margin.bottom);
            rightButtonStyle.fontSize = 10;
            if (GUILayout.Button("X", rightButtonStyle))
            {
                buttonEvent = ListButtonEvent.Delete;
            }

            return buttonEvent;
        }
    }
}