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
    using UnityEditor;

    public abstract class RenameOperationDrawer<T> : IRenameOperationDrawer where T : IRenameOperation
    {
        protected readonly Color32 ReplaceColor = new Color32(17, 138, 178, 255);
        protected readonly Color32 AddColor = new Color32(6, 214, 160, 255);
        protected readonly Color32 DeleteColor = new Color32(239, 71, 111, 255);
        protected readonly Color32 ModifyColor = new Color32(255, 209, 102, 255);

        protected const float LineSpacing = 2.0f;

        private readonly RectOffset Padding = new RectOffset(4, 4, 4, 6);
        private readonly float HeaderHeight = EditorGUIUtility.singleLineHeight;
        private const float HeaderAndContentSpacing = 2.0f;

        private string queuedControlToFocus;

        private T renameOperation;

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
        /// Gets the RenameOperation this drawer represents.
        /// </summary>
        public T RenameOperation
        {
            get
            {
                return this.renameOperation;
            }
        }

        /// <summary>
        /// Sets the RenameOperation instance represented by the drawer.
        /// </summary>
        /// <param name="renameOperationInstance">RenameOperation instance.</param>
        public void SetModel(IRenameOperation renameOperationInstance)
        {
            // This cast is a *bit* of an assumption, that the passed instance can be
            // downcasted to a more derived type (T : IRenameOperation) than IRenameOperation.
            this.renameOperation = (T)renameOperationInstance;
        }

        /// <summary>
        /// Gets the preferred height for the operation GUI.
        /// </summary>
        /// <returns>The preferred height for the GUI.</returns>
        public float GetPreferredHeight()
        {
            var headerHeight = EditorGUIUtility.singleLineHeight + HeaderAndContentSpacing;
            return headerHeight + this.GetPreferredHeightForContents() + Padding.top + Padding.bottom;
        }

        /// <summary>
        /// Draws the element, returning an event that indicates how the minibuttons are pressed.
        /// </summary>
        /// <param name="containingRect">The rect to draw the element inside.</param>
        /// <param name = "guiOptions">Options to use when drawing the operation GUI.</param>
        /// <returns>A ListButtonEvent indicating if a button was clicked.</returns>
        public RenameOperationSortingButtonEvent DrawGUI(Rect containingRect, RenameOperationGUIOptions guiOptions)
        {
            var paddedContainer = containingRect.AddPadding(Padding.left, Padding.right, Padding.top, Padding.bottom);
            var operationStyle = new GUIStyle("ScriptText");
            GUI.Box(containingRect, "", operationStyle);
            var headerRect = new Rect(paddedContainer);
            headerRect.height = HeaderHeight;
            RenameOperationSortingButtonEvent buttonEvent = this.DrawHeaderAndReorderButtons(
                                              headerRect,
                                              this.HeadingLabel,
                                              guiOptions.DisableUpButton,
                                              guiOptions.DisableDownButton);
            EditorGUI.indentLevel++;
            var contentsRect = new Rect(paddedContainer);
            contentsRect.y += headerRect.height + HeaderAndContentSpacing;
            contentsRect.height -= headerRect.height;
            this.DrawContents(contentsRect, guiOptions.ControlPrefix);
            EditorGUI.indentLevel--;

            var coloredHighlightRect = new Rect(containingRect);
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
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected abstract float GetPreferredHeightForContents();

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected abstract void DrawContents(Rect operationRect, int controlPrefix);

        /// <summary>
        /// Utility method for rename operations to use a consistent height based on a number of lines needed.
        /// </summary>
        /// <returns>The height for the number of lines.</returns>
        /// <param name="numLines">Number of lines to measure.</param>
        protected float CalculateGUIHeightForLines(int numLines)
        {
            // Unity includes spacing at the end of the last element, so we can just
            // add it into the line height.
            return (EditorGUIUtility.singleLineHeight + LineSpacing) * numLines;
        }

        /// <summary>
        /// Draws the header and reorder buttons.
        /// </summary>
        /// <returns>The header and reorder buttons.</returns>
        /// <param name="header">Header text to display.</param>
        /// <param name="disableUpButton">If set to <c>true</c> disable the MoveUp button.</param>
        /// <param name="disableDownButton">If set to <c>true</c> disable the MoveDown button.</param>
        protected RenameOperationSortingButtonEvent DrawHeaderAndReorderButtons(Rect containingRect, string header, bool disableUpButton, bool disableDownButton)
        {
            // Add some padding for the colored highlight
            var labelRect = new Rect(containingRect);
            var labelPaddingX = 4.0f;
            labelRect.x += labelPaddingX;
            labelRect.width -= labelPaddingX;
            EditorGUI.LabelField(labelRect, header, EditorStyles.boldLabel);

            RenameOperationSortingButtonEvent buttonEvent = RenameOperationSortingButtonEvent.None;
            var buttonPaddingTop = 1.0f;
            var buttonPaddingBottom = 1.0f;
            var buttonGroupWidth = 60.0f;
            var buttonRect = new Rect(
                labelRect.x + labelRect.width - buttonGroupWidth,
                labelRect.y + buttonPaddingTop,
                buttonGroupWidth,
                labelRect.height - (buttonPaddingBottom + buttonPaddingTop));
            buttonEvent = this.DrawReorderingButtons(buttonRect, disableUpButton, disableDownButton);

            return buttonEvent;
        }

        // <summary>
        /// Returns the path for the operation localized;
        /// </summary>
        /// <returns>The path for the operation localized</returns>
        /// <param name="folder">The folder of the operation.</param>
        /// <param name="name">The name of the operation</param>
        protected string GetOperationPath(string folder, string name)
        {
            return LocalizationManager.Instance.GetTranslation(folder) + "/" + LocalizationManager.Instance.GetTranslation(name);
        }

        private RenameOperationSortingButtonEvent DrawReorderingButtons(Rect containingRect, bool disableUpButton, bool disableDownButton)
        {
            const string BigUpArrowUnicode = "\u25B2";
            const string BigDownArrowUnicode = "\u25BC";
            RenameOperationSortingButtonEvent buttonEvent = RenameOperationSortingButtonEvent.None;

            EditorGUI.BeginDisabledGroup(disableUpButton);

            var leftButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButtonStyle.margin = new RectOffset(-1, -1, leftButtonStyle.margin.top, leftButtonStyle.margin.bottom);
            leftButtonStyle.fontSize = 8;
            var leftSplit = containingRect.GetSplitHorizontal(1, 3, 0.0f);
            if (GUI.Button(leftSplit, BigUpArrowUnicode, leftButtonStyle))
            {
                buttonEvent = RenameOperationSortingButtonEvent.MoveUp;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(disableDownButton);

            var midButtonStyle = new GUIStyle(EditorStyles.miniButtonMid);
            midButtonStyle.margin = new RectOffset(-1, -1, midButtonStyle.margin.top, midButtonStyle.margin.bottom);
            midButtonStyle.fontSize = 8;
            var midSplit = containingRect.GetSplitHorizontal(2, 3, 0.0f);
            if (GUI.Button(midSplit, BigDownArrowUnicode, midButtonStyle))
            {
                buttonEvent = RenameOperationSortingButtonEvent.MoveDown;
            }

            EditorGUI.EndDisabledGroup();

            var rightButtonStyle = new GUIStyle(EditorStyles.miniButtonRight);
            rightButtonStyle.margin = new RectOffset(-1, -1, rightButtonStyle.margin.top, rightButtonStyle.margin.bottom);
            rightButtonStyle.fontSize = 10;
            var rightSplit = containingRect.GetSplitHorizontal(3, 3, 0.0f);
            if (GUI.Button(rightSplit, "X", rightButtonStyle))
            {
                buttonEvent = RenameOperationSortingButtonEvent.Delete;
            }

            return buttonEvent;
        }
    }
}