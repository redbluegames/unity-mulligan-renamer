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
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The base RenameOperation used by BulkRenamer. All Operations derive from this,
    /// allowing for shared code.
    /// </summary>
    public abstract class RenameOperation : IRenameOperation
    {
        protected readonly Color32 ReplaceColor = new Color32(30, 139, 176, 255);
        protected readonly Color32 AddColor = new Color32(36, 213, 161, 255);
        protected readonly Color32 DeleteColor = new Color32(237, 74, 113, 255);
        protected readonly Color32 ModifyColor = new Color32(254, 208, 110, 255);

        private string queuedControlToFocus;

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
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public abstract string ControlToFocus { get; }

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
        public abstract string HeadingLabel { get; }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public abstract Color32 HighlightColor { get; }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A diff of the original name, renamed according to the rename operation's rules.</returns>
        public abstract RenameResult Rename(string input, int relativeCount);

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public abstract RenameOperation Clone();

        /// <summary>
        /// Draws the element as a GUI using EditorGUILayout calls. This should return a copy of the 
        /// Operation with the modified data. This way we mirror how regular GUI calls work.
        /// </summary>
        /// <param name = "guiOptions">Options to use when drawing the operation GUI.</param>
        /// <returns>A ListButtonEvent indicating if a button was clicked.</returns>
        public ListButtonEvent DrawGUI(GUIOptions guiOptions)
        {
            var operationStyle = new GUIStyle(GUI.skin.FindStyle("ScriptText"));
            operationStyle.stretchHeight = false;
            operationStyle.padding = new RectOffset(6, 6, 4, 4);
            Rect operationRect = EditorGUILayout.BeginVertical(operationStyle);
            ListButtonEvent buttonEvent = this.DrawHeaderAndReorderButtons(
                                              this.HeadingLabel,
                                              guiOptions.DisableUpButton,
                                              guiOptions.DisableDownButton);
            EditorGUI.indentLevel++;
            this.DrawContents(guiOptions.ControlPrefix);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            var coloredHighlightRect = new Rect(operationRect);
            coloredHighlightRect.yMin += 2.0f;
            coloredHighlightRect.yMax -= 1.0f;
            coloredHighlightRect.xMin += 2.0f;
            coloredHighlightRect.width = 3.0f;
            var oldColor = GUI.color;
            GUI.color = this.HighlightColor;
            GUI.DrawTexture(coloredHighlightRect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            return buttonEvent;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected abstract void DrawContents(int controlPrefix);

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

            // Leave some space for the colored highlight
            GUILayout.Space(4.0f);
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

        /// <summary>
        /// GUI options to apply when drawing a RenameOperation
        /// </summary>
        public class GUIOptions
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RenameOperation"/>
            /// should be drawn with the Up Button disabled.
            /// </summary>
            /// <value><c>true</c> if the up button should be disabled; otherwise, <c>false</c>.</value>
            public bool DisableUpButton { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RenameOperation"/>
            /// should be drawn with the Down Button disabled.
            /// </summary>
            /// <value><c>true</c> if the down button should be disabled; otherwise, <c>false</c>.</value>
            public bool DisableDownButton { get; set; }

            /// <summary>
            /// Gets or sets the prefix to use when setting control names for this <see cref="RenameOperation"/>
            /// </summary>
            /// <value>The control prefix.</value>
            public int ControlPrefix { get; set; }
        }
    }
}