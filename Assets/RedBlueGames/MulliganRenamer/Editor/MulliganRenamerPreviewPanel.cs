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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Mulligan renamer preview panel handles drawing of BulkRenamePreviews.
    /// </summary>
    public class MulliganRenamerPreviewPanel
    {
        const string BigUpArrowUnicode = "\u25B2";
        const string BigDownArrowUnicode = "\u25BC";
        private const float PreviewPanelFirstColumnMinSize = 50.0f;
        private const float PreviewRowHeight = 18.0f;

        /// <summary>
        /// Event fired when the AddSelectedObjects button is clicked
        /// </summary>
        public event System.Action AddSelectedObjectsClicked;

        /// <summary>
        /// Event fired when objects are dropped into the drag area
        /// </summary>
        public event System.Action<UnityEngine.Object[]> ObjectsDropped;

        /// <summary>
        /// Event fired when the RemoveAll button is clicked
        /// </summary>
        public event System.Action RemoveAllClicked;

        /// <summary>
        /// Event fired when an Object is removed
        /// </summary>
        public event System.Action<int> ObjectRemovedAtIndex;

        /// <summary>
        /// Event fired when an Object wants to change order
        /// </summary>
        public event System.Action<int, int> ChangeObjectOrder;

        public event System.Action Repaint;

        /// <summary>
        /// The validate object function, used to determine if objects should be
        /// allows to be added.
        /// </summary>
        public ValidateDraggedObject ValidateObject;

        public delegate bool ValidateDraggedObject(UnityEngine.Object obj);

        private GUIContents guiContents;
        private GUIStyles guiStyles;

        /// <summary>
        /// Gets or sets a value indicating whether this /// <see cref="T:RedBlueGames.MulliganRenamer.MulliganRenamerPreviewPanel"/>
        /// should disable its AddSelectedObjects button.
        /// </summary>
        public bool DisableAddSelectedObjectsButton { get; set; }

        /// <summary>
        /// Gets or sets the columns to show.
        /// </summary>
        public ColumnStyle ColumnsToShow { get; set; }

        /// <summary>
        /// Gets or sets the preview step index to show. -1 means only original and final names should be shown.
        /// </summary>
        public int PreviewStepIndexToShow { get; set; }

        /// <summary>
        /// Gets or sets the number of previously renamed objects.
        /// </summary>
        public int NumPreviouslyRenamedObjects { get; set; }

        public enum ColumnStyle
        {
            OriginalAndFinalOnly,
            Stepwise,
            StepwiseHideFinal
        }

        public enum PreviewRowResult
        {
            None,
            Delete,
            MoveUp,
            MoveDown
        }

        private PreviewPanelContentsLayout previewLayout;

        public MulliganRenamerPreviewPanel()
        {
            this.InitializeGUIStyles();
            this.InitializeGUIContents();
        }

        private static PreviewRowResult DrawPreviewRow(Rect rowRect, PreviewRowModel info, PreviewRowStyle style)
        {
            PreviewRowResult result = PreviewRowResult.None;

            var oldColor = GUI.color;
            GUI.color = style.BackgroundColor;
            GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            const float RowButtonsSize = 16f;
            const float InitialXOffset = 4f;

            // Space gives us a bit of padding or else we're just too bunched up to the side
            var deleteButtonRect = new Rect(rowRect);
            deleteButtonRect.x += InitialXOffset;
            deleteButtonRect.width = RowButtonsSize;
            deleteButtonRect.height = RowButtonsSize;
            deleteButtonRect.y += Mathf.Max(0, (rowRect.height - deleteButtonRect.height) / 2.0f);
            var deleteButtonStyle = new GUIStyle(EditorStyles.miniButton);
            deleteButtonStyle.padding = new RectOffset();
            if (GUI.Button(deleteButtonRect, "X", deleteButtonStyle))
            {
                result = PreviewRowResult.Delete;
            }

            deleteButtonRect.x += RowButtonsSize;
            GUI.enabled = !info.FirstElement;
            if (GUI.Button(deleteButtonRect, BigUpArrowUnicode, deleteButtonStyle))
            {
                result = PreviewRowResult.MoveUp;
            }

            deleteButtonRect.x += RowButtonsSize;
            GUI.enabled = !info.LastElement;
            if (GUI.Button(deleteButtonRect, BigDownArrowUnicode, deleteButtonStyle))
            {
                result = PreviewRowResult.MoveDown;
            }
            GUI.enabled = true;

            var widthOffset = RowButtonsSize + InitialXOffset;
            var warningRect = new Rect(deleteButtonRect);
            if(info.WarningIcon != null)
            {
                warningRect.y = rowRect.y;
                warningRect.x += deleteButtonRect.width;
                warningRect.width = RowButtonsSize;
                warningRect.height = RowButtonsSize;
            
                var content = new GUIContent(info.WarningIcon, info.WarningMessage);
                GUI.Box(warningRect, content, style.IconStyle);

                widthOffset += RowButtonsSize;
            }

            var iconRect = new Rect(warningRect);
            iconRect.x += RowButtonsSize;
            iconRect.width = RowButtonsSize;
            iconRect.height = RowButtonsSize;
            GUI.Box(iconRect, info.Icon, style.IconStyle);

            var firstColumnRect = new Rect(iconRect);
            firstColumnRect.x += iconRect.width;
            firstColumnRect.width = style.FirstColumnWidth - widthOffset;
            firstColumnRect.height = rowRect.height;
            if (style.FirstColumnWidth > 0)
            {
                EditorGUI.LabelField(firstColumnRect, info.NameBeforeStep, style.FirstColumnStyle);
            }
            
            var secondColumnRect = new Rect(firstColumnRect);
            secondColumnRect.x += firstColumnRect.width;
            secondColumnRect.width = style.SecondColumnWidth;
            secondColumnRect.height = rowRect.height;
            if (style.SecondColumnWidth > 0)
            {
                EditorGUI.LabelField(secondColumnRect, info.NameAtStep, style.SecondColumnStyle);
            }
            
            var thirdColumnRect = new Rect(secondColumnRect);
            thirdColumnRect.x += secondColumnRect.width;
            thirdColumnRect.width = rowRect.width;
            thirdColumnRect.height = rowRect.height;

            if (style.ThirdColumnWidth > 0)
            {
                EditorGUI.LabelField(thirdColumnRect, info.FinalName, style.ThirdColumnStyle);
            }

            if (GUI.Button(rowRect, "", GUIStyle.none))
            {
                EditorGUIUtility.PingObject(info.Object);
            }

            return result;
        }

        private void InitializeGUIContents()
        {
            this.guiContents = new GUIContents();

            this.guiContents.DropPrompt = new GUIContent(LocaleManager.Instance.GetTranslation("noObjectsSpecified"));
            this.guiContents.DropPromptHintInsideScroll = new GUIContent(LocaleManager.Instance.GetTranslation("addMoreObjectsDragHere"));
            this.guiContents.DropPromptHint = new GUIContent(LocaleManager.Instance.GetTranslation("addMoreObjectsDragPanel"));
            this.guiContents.DropPromptRepeat = new GUIContent(LocaleManager.Instance.GetTranslation("toRenameMoreObjects"));
        }

        private void InitializeGUIStyles()
        {
            this.guiStyles = new GUIStyles();

            this.guiStyles.Icon = GUIStyle.none;
            this.guiStyles.OriginalNameLabelUnModified = new GUIStyle(EditorStyles.label);
            this.guiStyles.OriginalNameLabelUnModified.richText = true;

            this.guiStyles.OriginalNameLabelWhenModified = new GUIStyle(EditorStyles.boldLabel);
            this.guiStyles.OriginalNameLabelWhenModified.richText = true;

            this.guiStyles.NewNameLabelUnModified = new GUIStyle(EditorStyles.label);
            this.guiStyles.NewNameLabelUnModified.richText = true;

            this.guiStyles.NewNameLabelModified = new GUIStyle(EditorStyles.boldLabel);
            this.guiStyles.NewNameLabelModified.richText = true;

            this.guiStyles.FinalNameLabelUnModified = new GUIStyle(EditorStyles.label);
            this.guiStyles.FinalNameLabelUnModified.richText = true;

            this.guiStyles.FinalNameLabelWhenModified = new GUIStyle(EditorStyles.boldLabel);
            this.guiStyles.FinalNameLabelWhenModified.richText = true;

            this.guiStyles.DropPrompt = new GUIStyle(EditorStyles.label);
            this.guiStyles.DropPrompt.alignment = TextAnchor.MiddleCenter;
            this.guiStyles.DropPrompt.wordWrap = true;
            this.guiStyles.DropPromptRepeat = new GUIStyle(EditorStyles.label);
            this.guiStyles.DropPromptRepeat.alignment = TextAnchor.MiddleCenter;

            this.guiStyles.DropPromptHintInsideScroll = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            this.guiStyles.DropPromptHint = new GUIStyle(EditorStyles.wordWrappedMiniLabel);

            this.guiStyles.RenameSuccessPrompt = new GUIStyle(EditorStyles.label);
            this.guiStyles.RenameSuccessPrompt.alignment = TextAnchor.MiddleCenter;
            this.guiStyles.RenameSuccessPrompt.richText = true;
            this.guiStyles.RenameSuccessPrompt.fontSize = 16;

            var previewHeaderStyle = new GUIStyle(EditorStyles.toolbar);
            var previewHeaderMargin = new RectOffset();
            previewHeaderMargin = previewHeaderStyle.margin;
            previewHeaderMargin.left = 1;
            previewHeaderMargin.right = 1;
            previewHeaderStyle.margin = previewHeaderMargin;
            this.guiStyles.PreviewHeader = previewHeaderStyle;

            if (EditorGUIUtility.isProSkin)
            {
                string styleName = string.Empty;
#if UNITY_5
                styleName = "AnimationCurveEditorBackground";
#else
                styleName = "CurveEditorBackground";
#endif

                this.guiStyles.PreviewScroll = new GUIStyle(styleName);

                this.guiStyles.PreviewRowBackgroundEven = new Color(0.3f, 0.3f, 0.3f, 0.2f);

                this.guiStyles.InsertionTextColor = new Color32(6, 214, 160, 255);
                this.guiStyles.DeletionTextColor = new Color32(239, 71, 111, 255);
            }
            else
            {
                this.guiStyles.PreviewScroll = new GUIStyle(EditorStyles.textArea);

                this.guiStyles.PreviewRowBackgroundEven = new Color(0.6f, 0.6f, 0.6f, 0.2f);

                this.guiStyles.InsertionTextColor = new Color32(0, 140, 104, 255);
                this.guiStyles.DeletionTextColor = new Color32(189, 47, 79, 255);
            }

            this.guiStyles.PreviewRowBackgroundOdd = Color.clear;
        }

        /// <summary>
        /// Draw the specified BulkRenamePreview in the given rect, and returns the new scroll position.
        /// </summary>
        /// <returns>The new scroll position.</returns>
        /// <param name="previewPanelRect">Preview panel rect.</param>
        /// <param name="previewPanelScrollPosition">Preview panel scroll position.</param>
        /// <param name="preview">Preview to draw.</param>
        public Vector2 Draw(Rect previewPanelRect, Vector2 previewPanelScrollPosition, BulkRenamePreview preview)
        {
            var spaceBetweenFooterAndScrollview = 2.0f;
            var panelFooterToolbar = new Rect(previewPanelRect);
            panelFooterToolbar.height = EditorGUIUtility.singleLineHeight;
            panelFooterToolbar.y += (previewPanelRect.height + spaceBetweenFooterAndScrollview) - panelFooterToolbar.height;

            var scrollViewRect = new Rect(previewPanelRect);
            scrollViewRect.height -= (panelFooterToolbar.height + spaceBetweenFooterAndScrollview);

            GUI.Box(scrollViewRect, "", this.guiStyles.PreviewScroll);

            var newScrollPosition = previewPanelScrollPosition;
            if (preview.NumObjects == 0)
            {
                this.DrawPreviewPanelContentsEmpty(scrollViewRect);
            }
            else
            {
                var scrollLayout = new PreviewPanelLayout(scrollViewRect);

                // Show the one that doesn't quite fit by subtracting one
                var firstItemIndex = Mathf.Max(Mathf.FloorToInt(previewPanelScrollPosition.y / PreviewRowHeight) - 1, 0);

                // Add one for the one that's off screen above, and one for the one below. I think?
                var numItems = Mathf.CeilToInt(scrollLayout.ScrollRect.height / PreviewRowHeight) + 2;

                var previewContents = PreviewPanelContents.CreatePreviewContentsForObjects(
                    preview,
                    firstItemIndex,
                    numItems,
                    this.PreviewStepIndexToShow,
                    this.guiStyles.DeletionTextColor,
                    this.guiStyles.InsertionTextColor);

                bool shouldShowSecondColumn = this.ColumnsToShow != ColumnStyle.OriginalAndFinalOnly;
                bool shouldShowThirdColumn = this.ColumnsToShow != ColumnStyle.StepwiseHideFinal;
                if (previewLayout == null)
                {
                    previewLayout = new PreviewPanelContentsLayout(
                        scrollLayout.ScrollRect,
                        previewContents,
                        shouldShowSecondColumn,
                        shouldShowThirdColumn);
                }
                else
                {
                    previewLayout.UpdateContentsLayout(scrollLayout.ScrollRect, shouldShowSecondColumn, shouldShowThirdColumn);
                }

                newScrollPosition = this.DrawPreviewPanelContentsWithItems(
                    scrollLayout,
                    previewLayout,
                    previewPanelScrollPosition,
                    previewContents,
                    this.PreviewStepIndexToShow,
                    shouldShowSecondColumn,
                    shouldShowThirdColumn);

                var buttonSpacing = 2.0f;
                var rightPadding = 2.0f;
                var addSelectedObjectsButtonRect = new Rect(panelFooterToolbar);
                addSelectedObjectsButtonRect.width = 150.0f;
                addSelectedObjectsButtonRect.x = panelFooterToolbar.xMax - rightPadding - addSelectedObjectsButtonRect.width;

                var removeAllButtonRect = new Rect(addSelectedObjectsButtonRect);
                removeAllButtonRect.width = 100.0f;
                removeAllButtonRect.x -= (removeAllButtonRect.width + buttonSpacing);
                if (GUI.Button(removeAllButtonRect, LocaleManager.Instance.GetTranslation("removeAll")))
                {
                    if (this.RemoveAllClicked != null)
                    {
                        this.RemoveAllClicked.Invoke();
                    }
                }

                this.DrawAddSelectedObjectsButton(addSelectedObjectsButtonRect);

                if (!scrollLayout.ContentsFitWithoutAnyScrolling(previewLayout))
                {
                    var hintRect = new Rect(scrollViewRect);
                    hintRect.height = EditorGUIUtility.singleLineHeight * 2.0f;
                    hintRect.y += scrollViewRect.height;
                    hintRect.width = scrollViewRect.width - addSelectedObjectsButtonRect.width - removeAllButtonRect.width - buttonSpacing;
                    EditorGUI.LabelField(hintRect, this.guiContents.DropPromptHint, this.guiStyles.DropPromptHint);
                }
            }

            var draggedObjects = this.GetDraggedObjectsOverRect(scrollViewRect);
            if (draggedObjects.Count > 0)
            {
                if (this.ObjectsDropped != null)
                {
                    this.ObjectsDropped.Invoke(draggedObjects.ToArray());
                }
            }

            return newScrollPosition;
        }

        private void DrawPreviewPanelContentsEmpty(Rect previewPanelRect)
        {
            var showSuccessfulRenameLabels = this.NumPreviouslyRenamedObjects > 0;
            var labelRect = new Rect(previewPanelRect);
            labelRect.height = EditorGUIUtility.singleLineHeight * 2.0f;
            labelRect.y = previewPanelRect.height / 2.0f - labelRect.height;

            var promptRect = new Rect(labelRect);
            promptRect.y += labelRect.height;
            if (!showSuccessfulRenameLabels)
            {
                promptRect.height = 0.0f;
            }

            var buttonRect = new Rect(promptRect);
            buttonRect.y += promptRect.height;
            buttonRect.width = 200.0f;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            buttonRect.x += previewPanelRect.width / 2.0f - buttonRect.width * 0.5f;

            if (showSuccessfulRenameLabels)
            {
                var oldColor = GUI.contentColor;
                GUI.contentColor = this.guiStyles.InsertionTextColor;
                string noun;
                if (this.NumPreviouslyRenamedObjects == 1)
                {
                    noun = LocaleManager.Instance.GetTranslation("object");
                }
                else
                {
                    noun = LocaleManager.Instance.GetTranslation("objects");
                }

                var renameSuccessContent = new GUIContent(string.Format("{0} {1} {2}", this.NumPreviouslyRenamedObjects, noun, LocaleManager.Instance.GetTranslation("renamed")));
                EditorGUI.LabelField(labelRect, renameSuccessContent, this.guiStyles.RenameSuccessPrompt);
                GUI.contentColor = oldColor;

                EditorGUI.LabelField(promptRect, this.guiContents.DropPromptRepeat, this.guiStyles.DropPromptRepeat);
            }
            else
            {
                EditorGUI.LabelField(labelRect, this.guiContents.DropPrompt, this.guiStyles.DropPrompt);
            }

            this.DrawAddSelectedObjectsButton(buttonRect);
        }

        private Vector2 DrawPreviewPanelContentsWithItems(
            PreviewPanelLayout scrollLayout,
            PreviewPanelContentsLayout contentsLayout,
            Vector2 previewPanelScrollPosition,
            PreviewPanelContents previewContents,
            int renameStep,
            bool shouldShowSecondColumn,
            bool shouldShowThirdColumn)
        {
            // WORKAROUND FOR 5.5.5: Somehow you could "scroll" the preview area, even
            // when there was nothing to scroll. Force it to not think it's scrolled because
            // that was screwing up the Header.
            if (scrollLayout.ContentsFitWithoutScrollingHorizontally(contentsLayout))
            {
                previewPanelScrollPosition.x = 0;
            }

            string originalNameColumnHeader = LocaleManager.Instance.GetTranslation(renameStep < 1 ? "original" : "before");
            string newNameColumnHeader = LocaleManager.Instance.GetTranslation("after");
            this.DrawPreviewHeader(
                scrollLayout.HeaderRect,
                -previewPanelScrollPosition.x,
                originalNameColumnHeader,
                newNameColumnHeader,
                contentsLayout.FirstColumnWidth,
                contentsLayout.SecondColumnWidth,
                contentsLayout.ThirdColumnWidth);

            var newScrollPosition = GUI.BeginScrollView(
                scrollLayout.ScrollRect,
                previewPanelScrollPosition,
                contentsLayout.Rect);

            var rowRect = new Rect(scrollLayout.ScrollRect);
            rowRect.width = Mathf.Max(contentsLayout.Rect.width, scrollLayout.ScrollRect.width);
            this.DrawPreviewRows(rowRect, previewContents, contentsLayout, shouldShowSecondColumn, shouldShowThirdColumn);

            // Add the hint into the scroll view if there's room
            if (scrollLayout.ContentsFitWithoutAnyScrolling(contentsLayout))
            {
                EditorGUI.LabelField(scrollLayout.HintRect, this.guiContents.DropPromptHintInsideScroll, this.guiStyles.DropPromptHintInsideScroll);
            }

            GUI.EndScrollView();

            this.DrawDividers(newScrollPosition, scrollLayout, contentsLayout, shouldShowThirdColumn);

            GUI.EndGroup();

            return newScrollPosition;
        }

        private void DrawPreviewHeader(
            Rect headerRect,
            float scrollOffsetX,
            string firstColumnName,
            string secondColumnName,
            float firstColumnWidth,
            float secondColumnWidth,
            float thirdColumnWidth)
        {
            var relativeHeaderRect = new Rect(headerRect);
            relativeHeaderRect.x = scrollOffsetX;
            relativeHeaderRect.y = 0.0f;

            var backgroundRect = new Rect(headerRect);
            backgroundRect.height -= 2.0f;
            backgroundRect.y += 1.0f;
            backgroundRect.width -= 2.0f;
            backgroundRect.x += 1.0f;

            var firstColumnRect = new Rect(relativeHeaderRect);
            firstColumnRect.y += 2.0f;

            // Space gives us a bit of padding or else we're just too bunched up to the side
            // It also includes space for the delete button and icons.
            var leftSpace = 52.0f;
            firstColumnRect.x += leftSpace;
            firstColumnRect.width = firstColumnWidth;

            var secondColumnRect = new Rect(firstColumnRect);
            secondColumnRect.x += firstColumnRect.width;
            secondColumnRect.width = secondColumnWidth;

            var thirdColumnRect = new Rect(secondColumnRect);
            thirdColumnRect.x += secondColumnRect.width;
            thirdColumnRect.width = thirdColumnWidth;

            GUI.Box(backgroundRect, "", this.guiStyles.PreviewHeader);

            // Group lets us clip the header and scroll it with the scroll view
            GUI.BeginGroup(headerRect);
            EditorGUI.LabelField(firstColumnRect, firstColumnName, EditorStyles.boldLabel);

            if (secondColumnWidth > 0.0f)
            {
                EditorGUI.LabelField(secondColumnRect, secondColumnName, EditorStyles.boldLabel);
            }

            if (thirdColumnWidth > 0.0f)
            {
                EditorGUI.LabelField(thirdColumnRect, LocaleManager.Instance.GetTranslation("finalName"), EditorStyles.boldLabel);
            }

            GUI.EndGroup();
        }

        private void DrawPreviewRows(Rect previewRowsRect, PreviewPanelContents previewContents, PreviewPanelContentsLayout layout, bool showSecondColumn, bool showThirdColumn)
        {
            for (int i = 0; i < previewContents.NumVisibleRows; ++i)
            {
                var content = previewContents[i];
                var previewRowStyle = new PreviewRowStyle();
                previewRowStyle.IconStyle = this.guiStyles.Icon;

                previewRowStyle.FirstColumnStyle = content.NameChangedThisStep ?
                    this.guiStyles.OriginalNameLabelWhenModified :
                    this.guiStyles.OriginalNameLabelUnModified;
                previewRowStyle.FirstColumnWidth = layout.FirstColumnWidth;

                previewRowStyle.SecondColumnStyle = content.NameChangedThisStep ?
                    this.guiStyles.NewNameLabelModified :
                    this.guiStyles.NewNameLabelUnModified;

                previewRowStyle.SecondColumnWidth = layout.SecondColumnWidth;

                previewRowStyle.ThirdColumnStyle = content.NameChangedThisStep ?
                    this.guiStyles.FinalNameLabelWhenModified :
                    this.guiStyles.FinalNameLabelUnModified;

                previewRowStyle.ThirdColumnWidth = layout.ThirdColumnWidth;

                previewRowStyle.BackgroundColor = i % 2 == 0 ? this.guiStyles.PreviewRowBackgroundEven : this.guiStyles.PreviewRowBackgroundOdd;

                var rowRect = new Rect(previewRowsRect);
                rowRect.height = PreviewRowHeight;
                rowRect.y = previewRowsRect.y + (content.IndexInPreview * rowRect.height);
                switch (DrawPreviewRow(rowRect, content, previewRowStyle))
                {
                    case PreviewRowResult.Delete:
                        if (this.ObjectRemovedAtIndex != null)
                        {
                            this.ObjectRemovedAtIndex.Invoke(i);
                        }
                        break;
                    case PreviewRowResult.MoveUp:
                        if (this.ChangeObjectOrder != null)
                        {
                            this.ChangeObjectOrder.Invoke(i, i - 1);
                        }
                        break;
                    case PreviewRowResult.MoveDown:
                        if (this.ChangeObjectOrder != null)
                        {
                            this.ChangeObjectOrder.Invoke(i, i + 1);
                        }
                        break;
                    default:
                        continue;
                }

                break;
            }
        }

        private void DrawDividers(
            Vector2 newScrollPosition,
            PreviewPanelLayout scrollLayout,
            PreviewPanelContentsLayout contentsLayout,
            bool shouldShowThirdColumn)
        {
            // Put dividers in group so that they scroll (horizontally)
            var dividerGroup = new Rect(scrollLayout.ScrollRect);
            dividerGroup.y -= scrollLayout.HeaderRect.height;
            dividerGroup.height += scrollLayout.HeaderRect.height;
            GUI.BeginGroup(dividerGroup);
            var oldColor = GUI.color;
            if (EditorGUIUtility.isProSkin)
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            }

            // Add 1 into y for the position so that it doesn't render on the panel's border
            var dividerHeight = dividerGroup.height;

            // Unity scroll view bars overlap the scroll rect when they render, so we have to shorten the dividers
            if (!scrollLayout.ContentsFitWithoutScrollingHorizontally(contentsLayout))
            {
                var scrollbarHeight = 14.0f;
                dividerHeight -= scrollbarHeight;
            }

            var firstDividerRect = new Rect(
                -newScrollPosition.x + contentsLayout.FirstColumnWidth + contentsLayout.IconSize,
                0.0f,
                2.0f,
                dividerHeight - 1.0f);

            if (DrawDividerAndCheckResize(firstDividerRect))
                resizeFirstDivider = true;

            if (resizeFirstDivider)
                ChangeColumnSizeAndRepaint(contentsLayout.ChangeFirstColumnWidth,Event.current.mousePosition.x - contentsLayout.IconSize);

            if (shouldShowThirdColumn)
            {
                var secondDividerRect = new Rect(firstDividerRect);
                secondDividerRect.x += contentsLayout.SecondColumnWidth;

                if (DrawDividerAndCheckResize(secondDividerRect))
                    resizeSecondDivider = true;

                if (resizeSecondDivider)
                    ChangeColumnSizeAndRepaint(contentsLayout.ChangeSecondColumnWidth, Event.current.mousePosition.x - contentsLayout.FirstColumnWidth - contentsLayout.IconSize);
            }

            if (Event.current.rawType == EventType.MouseUp)
            {
                resizeFirstDivider = false;
                resizeSecondDivider = false;
            }

            GUI.color = oldColor;
        }

        private void ChangeColumnSizeAndRepaint(Action<float> method, float value)
        {
            method(value);
            Repaint.Invoke();
        }

        private bool resizeFirstDivider;
        private bool resizeSecondDivider;
        private bool DrawDividerAndCheckResize(Rect rect)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        private void DrawAddSelectedObjectsButton(Rect buttonRect)
        {
            EditorGUI.BeginDisabledGroup(DisableAddSelectedObjectsButton);
            if (GUI.Button(buttonRect, LocaleManager.Instance.GetTranslation("addSelectedObjects")))
            {
                if (this.AddSelectedObjectsClicked != null)
                {
                    this.AddSelectedObjectsClicked.Invoke();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private List<UnityEngine.Object> GetDraggedObjectsOverRect(Rect dropArea)
        {
            Event currentEvent = Event.current;

            var droppedObjects = new List<UnityEngine.Object>();
            if (!dropArea.Contains(currentEvent.mousePosition))
            {
                return droppedObjects;
            }

            var validDraggedObjects = new List<UnityEngine.Object>();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (this.ValidateObject == null || this.ValidateObject(obj))
                {
                    validDraggedObjects.Add(obj);
                }
            }

            var isDraggingValidAssets = (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform) &&
                                        validDraggedObjects.Count > 0;
            if (isDraggingValidAssets)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    droppedObjects.AddRange(validDraggedObjects);
                }

                currentEvent.Use();
            }

            return droppedObjects;
        }

        private class PreviewPanelLayout
        {
            public Rect HeaderRect { get; private set; }
            public Rect ScrollRect { get; private set; }
            public Rect HintRect { get; private set; }

            public PreviewPanelLayout(Rect previewPanelRect)
            {
                var spaceBetweenHeaderAndScroll = 1.0f;
                var headerRect = new Rect(previewPanelRect);
                headerRect.height = 18.0f;
                this.HeaderRect = headerRect;

                var scrollRect = new Rect(previewPanelRect);
                scrollRect.height -= (headerRect.height + spaceBetweenHeaderAndScroll);
                scrollRect.y += (headerRect.height + spaceBetweenHeaderAndScroll);
                this.ScrollRect = scrollRect;

                var hintRect = new Rect(scrollRect);
                hintRect.height = EditorGUIUtility.singleLineHeight;
                hintRect.y += scrollRect.height - hintRect.height;
                this.HintRect = hintRect;
            }

            public bool ContentsFitWithoutScrollingHorizontally(PreviewPanelContentsLayout contentsLayout)
            {
                return contentsLayout.Rect.width <= this.ScrollRect.width;
            }

            public bool ContentsFitWithoutScrollingVertically(PreviewPanelContentsLayout contentsLayout)
            {
                return contentsLayout.Rect.height + this.HintRect.height <= this.ScrollRect.height;
            }

            public bool ContentsFitWithoutAnyScrolling(PreviewPanelContentsLayout contentsLayout)
            {
                return this.ContentsFitWithoutScrollingHorizontally(contentsLayout) &&
                           this.ContentsFitWithoutScrollingVertically(contentsLayout);
            }
        }

        private class PreviewPanelContentsLayout
        {
            private const float MinColumnWidth = 100f;

            public float IconSize
            {
                get
                {
                    // For now we just use a flat 48 icon size.
                    return 48.0f;
                }
            }

            public Rect Rect { get; private set; }

            public float FirstColumnWidth { get; private set; }

            public float SecondColumnWidth { get; private set; }

            public float ThirdColumnWidth { get; private set; }

            private bool IsShowingSecondColumn
            {
                get { return SecondColumnWidth > 0f; }
            }

            private bool IsShowingThirdColumn
            {
                get { return ThirdColumnWidth > 0f; }
            }

            public PreviewPanelContentsLayout(Rect scrollRect, PreviewPanelContents previewContents, bool shouldShowSecondColumn, bool shouldShowThirdColumn)
            {
                this.FirstColumnWidth = previewContents.LongestOriginalNameWidth;
                this.SecondColumnWidth = shouldShowSecondColumn ? previewContents.LongestNewNameWidth : 0.0f;
                this.ThirdColumnWidth = shouldShowThirdColumn ? Mathf.Max(previewContents.LongestFinalNameWidth, 80.0f) : 0.0f;
                
                var rect = new Rect(scrollRect);
                rect.height = PreviewRowHeight * previewContents.TotalNumRows;
                this.Rect = rect;
            }

            public void UpdateContentsLayout(Rect scrollRect, bool shouldShowSecondColumn, bool shouldShowThirdColumn)
            {
                this.Rect = new Rect(scrollRect);

                ToggleSecondColumn(shouldShowSecondColumn);
                ToggleThirdColumn(shouldShowThirdColumn);
            }

            private void ToggleSecondColumn(bool active)
            {
                if (active == IsShowingSecondColumn)
                    return;

                if (!active)
                {
                    FirstColumnWidth = Rect.width;
                    SecondColumnWidth = 0f;
                    ThirdColumnWidth = 0f;
                }
                else
                {
                    var remainingSize = Rect.width - FirstColumnWidth;
                    SecondColumnWidth = remainingSize;
                }
            }

            private void ToggleThirdColumn(bool active)
            {
                if (active == IsShowingThirdColumn)
                    return;

                if (!active)
                {
                    SecondColumnWidth = Rect.width - FirstColumnWidth;
                    ThirdColumnWidth = 0;
                }
                else
                {
                    var dividedSize = (Rect.width - FirstColumnWidth) / 2f;
                    SecondColumnWidth = dividedSize;
                    ThirdColumnWidth = dividedSize;
                }
            }

            public void ChangeFirstColumnWidth(float width)
            {
                var maxWidth = Rect.width - ThirdColumnWidth - MinColumnWidth - IconSize; 
                FirstColumnWidth = Mathf.Clamp(width, MinColumnWidth, maxWidth);
                SecondColumnWidth = Rect.width - (FirstColumnWidth + ThirdColumnWidth);
            }

            public void ChangeSecondColumnWidth(float width)
            {
                var maxWidth = Rect.width - MinColumnWidth - FirstColumnWidth - IconSize;
                SecondColumnWidth = Mathf.Clamp(width, MinColumnWidth, maxWidth);
                ThirdColumnWidth = Rect.width - (FirstColumnWidth + SecondColumnWidth);
            }
        }

        private class PreviewPanelContents
        {
            private const float MinColumnWidth = 150.0f;

            public float LongestOriginalNameWidth { get; private set; }

            public float LongestNewNameWidth { get; private set; }

            public float LongestFinalNameWidth { get; private set; }

            public int TotalNumRows { get; set; }

            public int NumVisibleRows
            {
                get
                {
                    return this.PreviewRowInfos.Length;
                }
            }

            private PreviewRowModel[] PreviewRowInfos { get; set; }

            public PreviewRowModel this[int index]
            {
                get
                {
                    if (index >= 0 && index < this.PreviewRowInfos.Length)
                    {
                        return this.PreviewRowInfos[index];
                    }
                    else
                    {
                        throw new System.IndexOutOfRangeException(
                            LocaleManager.Instance.GetTranslation("errorTryingToAccessModel") + index);
                    }
                }
            }

            public static PreviewPanelContents CreatePreviewContentsForObjects(
                BulkRenamePreview preview,
                int firstPreviewIndex,
                int numObjectsToShow,
                int stepIndex,
                Color deletionColor,
                Color insertionColor)
            {
                var previewPanelContents = new PreviewPanelContents();
                var numVisibleObjects = Mathf.Min(numObjectsToShow, preview.NumObjects);
                previewPanelContents.PreviewRowInfos = new PreviewRowModel[numVisibleObjects];

                for (int j = 0; j < numVisibleObjects && j < preview.NumObjects - firstPreviewIndex; ++j)
                {
                    var info = new PreviewRowModel();
                    var indexOfVisibleObject = firstPreviewIndex + j;
                    var previewForIndex = preview.GetPreviewAtIndex(indexOfVisibleObject);
                    var originalName = stepIndex >= 0 && stepIndex < preview.NumSteps ?
                        previewForIndex.RenameResultSequence.GetNameBeforeAtStep(stepIndex, deletionColor) :
                        previewForIndex.RenameResultSequence.OriginalName;
                    info.NameBeforeStep = originalName;

                    var nameAtStep = stepIndex >= 0 && stepIndex < preview.NumSteps ?
                        previewForIndex.RenameResultSequence.GetNewNameAtStep(stepIndex, insertionColor) :
                        previewForIndex.RenameResultSequence.NewName;
                    info.NameAtStep = nameAtStep;

                    info.FinalName = previewForIndex.RenameResultSequence.NewName;

                    info.Icon = previewForIndex.ObjectToRename.GetEditorIcon();
                    info.Object = previewForIndex.ObjectToRename;

                    if (previewForIndex.HasWarnings || preview.WillRenameCollideWithExistingAsset(previewForIndex))
                    {
                        info.WarningIcon = (Texture2D)EditorGUIUtility.Load("icons/console.warnicon.sml.png");
                        if (previewForIndex.HasWarnings)
                        {
                            info.WarningMessage = GetWarningMessageForRenamePreview(previewForIndex);
                        }
                        else
                        {
                            info.WarningMessage = LocaleManager.Instance.GetTranslation("warningNewNameMatchesExisting");
                        }
                    }
                    else
                    {
                        info.WarningIcon = null;
                        info.WarningMessage = string.Empty;
                    }

                    info.IndexInPreview = indexOfVisibleObject;
                    info.FirstElement = j == 0;
                    info.LastElement = j == (numVisibleObjects - 1);
                    previewPanelContents.PreviewRowInfos[j] = info;
                }

                // Note that CalcSize is very slow, so it is a problem to do it to the entire list of objects...
                // For now we only measure the ones that are visible. It causes the columns to resize as you scroll,
                // but that's not the worst.
                float paddingScaleForBold = 1.11f;
                previewPanelContents.LongestOriginalNameWidth = 0.0f;
                previewPanelContents.LongestNewNameWidth = 0.0f;
                foreach (var previewRowInfo in previewPanelContents.PreviewRowInfos)
                {
                    var labelStyle = GUI.skin.label;
                    labelStyle.richText = true;
                    float originalNameWidth = labelStyle.CalcSize(
                              new GUIContent(previewRowInfo.NameBeforeStep)).x * paddingScaleForBold;
                    if (originalNameWidth > previewPanelContents.LongestOriginalNameWidth)
                    {
                        previewPanelContents.LongestOriginalNameWidth = originalNameWidth;
                    }

                    float newNameWidth = labelStyle.CalcSize(
                                             new GUIContent(previewRowInfo.NameAtStep)).x * paddingScaleForBold;
                    if (newNameWidth > previewPanelContents.LongestNewNameWidth)
                    {
                        previewPanelContents.LongestNewNameWidth = newNameWidth;
                    }

                    float finalNameWidth = labelStyle.CalcSize(
                                             new GUIContent(previewRowInfo.FinalName)).x * paddingScaleForBold;
                    if (finalNameWidth > previewPanelContents.LongestFinalNameWidth)
                    {
                        previewPanelContents.LongestFinalNameWidth = finalNameWidth;
                    }
                }

                previewPanelContents.LongestOriginalNameWidth = Mathf.Max(MinColumnWidth, previewPanelContents.LongestOriginalNameWidth);
                previewPanelContents.LongestNewNameWidth = Mathf.Max(MinColumnWidth, previewPanelContents.LongestNewNameWidth);
                previewPanelContents.LongestFinalNameWidth = previewPanelContents.LongestFinalNameWidth;

                previewPanelContents.TotalNumRows = preview.NumObjects;

                return previewPanelContents;
            }

            private static string GetWarningMessageForRenamePreview(RenamePreview preview)
            {
                if (preview.HasInvalidEmptyFinalName)
                {
                    return LocaleManager.Instance.GetTranslation("assetBlankName");
                }
                else if (preview.FinalNameContainsInvalidCharacters)
                {
                    return LocaleManager.Instance.GetTranslation("nameIncludeInvalidCharacter");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private struct PreviewRowModel
        {
            public UnityEngine.Object Object { get; set; }

            public Texture Icon { get; set; }

            public Texture WarningIcon { get; set; }

            public string WarningMessage { get; set; }

            public string NameBeforeStep { get; set; }

            public string NameAtStep { get; set; }

            public string FinalName { get; set; }

            public int IndexInPreview { get; set; }

            public bool LastElement { get; set; }

            public bool FirstElement { get; set; }

            public bool NameChangedThisStep
            {
                get
                {
                    return this.NameBeforeStep != this.NameAtStep;
                }
            }
        }

        private struct PreviewRowStyle
        {
            public GUIStyle IconStyle { get; set; }

            public GUIStyle FirstColumnStyle { get; set; }

            public float FirstColumnWidth { get; set; }

            public GUIStyle SecondColumnStyle { get; set; }

            public float SecondColumnWidth { get; set; }

            public GUIStyle ThirdColumnStyle { get; set; }

            public float ThirdColumnWidth { get; set; }

            public Color BackgroundColor { get; set; }
        }

        private class GUIStyles
        {
            public GUIStyle PreviewScroll { get; set; }

            public GUIStyle Icon { get; set; }

            public GUIStyle OriginalNameLabelUnModified { get; set; }

            public GUIStyle OriginalNameLabelWhenModified { get; set; }

            public GUIStyle NewNameLabelUnModified { get; set; }

            public GUIStyle NewNameLabelModified { get; set; }

            public GUIStyle FinalNameLabelUnModified { get; set; }

            public GUIStyle FinalNameLabelWhenModified { get; set; }

            public GUIStyle DropPrompt { get; set; }

            public GUIStyle DropPromptHint { get; set; }

            public GUIStyle DropPromptHintInsideScroll { get; set; }

            public GUIStyle DropPromptRepeat { get; set; }

            public GUIStyle RenameSuccessPrompt { get; set; }

            public GUIStyle PreviewHeader { get; set; }

            public Color PreviewRowBackgroundOdd { get; set; }

            public Color PreviewRowBackgroundEven { get; set; }

            public Color InsertionTextColor { get; set; }

            public Color DeletionTextColor { get; set; }
        }

        private class GUIContents
        {
            public GUIContent DropPrompt { get; set; }

            public GUIContent DropPromptRepeat { get; set; }

            public GUIContent DropPromptHint { get; set; }

            public GUIContent DropPromptHintInsideScroll { get; set; }
        }
    }
}