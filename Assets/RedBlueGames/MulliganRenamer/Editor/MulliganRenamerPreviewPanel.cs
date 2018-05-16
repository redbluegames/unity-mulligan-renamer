namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class MulliganRenamerPreviewPanel
    {
        // TODO: Cleanup -
        // ShowPreviewSteps. 
        // StepIndex (DrawPreviewRow should just be told what to draw, it shouldn't figure it out.
        // FocusedRenameOpIndex
        // IsPreviewStepModePreference (again, it shouldn't infer how to draw it should just be told)
        // NumRenameOperations

        // NumPreviouslyRenamedObjects?

        // previewPanelScrollPosition doesn't need to get saved, I don't think...
        //      YUCK "Contents fit without scrolling" also makes it impossible to return the scroll position. But SHOULD Draw return scroll position??

        public event System.Action AddSelectedObjectsClicked;
        public event System.Action<UnityEngine.Object[]> ObjectsDropped;
        public event System.Action RemoveAllClicked;
        public event System.Action<int> ObjectRemoved;

        public ValidateDraggedObject ValidateObject;

        public delegate bool ValidateDraggedObject(UnityEngine.Object obj);

        private const float PreviewPanelFirstColumnMinSize = 50.0f;
        private const float PreviewRowHeight = 18.0f;

        private GUIContents guiContents;
        private GUIStyles guiStyles;

        public bool DisableAddSelectedObjectsButton { get; set; }
        public int NumRenameOperations { get; set; }
        public bool IsPreviewStepModePreference { get; set; }
        public int FocusedRenameOpIndex { get; set; }
        public bool ShowPreviewSteps { get; set; }
        public int NumPreviouslyRenamedObjects { get; set; }

        public MulliganRenamerPreviewPanel()
        {
            this.InitializeGUIStyles();
            this.InitializeGUIContents();
        }

        private static bool DrawPreviewRow(Rect rowRect, int previewStepIndex, PreviewRowModel info, PreviewRowStyle style)
        {
            bool isDeleteClicked = false;

            var oldColor = GUI.color;
            GUI.color = style.BackgroundColor;
            GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            // Space gives us a bit of padding or else we're just too bunched up to the side
            var deleteButtonRect = new Rect(rowRect);
            deleteButtonRect.x += 4.0f;
            deleteButtonRect.width = 16.0f;
            deleteButtonRect.height = 16.0f;
            deleteButtonRect.y += Mathf.Max(0, (rowRect.height - deleteButtonRect.height) / 2.0f);
            if (GUI.Button(deleteButtonRect, "x", EditorStyles.miniButton))
            {
                isDeleteClicked = true;
            }

            var warningRect = new Rect(deleteButtonRect);
            warningRect.y = rowRect.y;
            warningRect.x += deleteButtonRect.width;
            warningRect.width = 16.0f;
            warningRect.height = 16.0f;
            if (info.WarningIcon != null)
            {
                var content = new GUIContent(info.WarningIcon, info.WarningMessage);
                GUI.Box(warningRect, content, style.IconStyle);
            }

            var iconRect = new Rect(warningRect);
            iconRect.x += warningRect.width;
            iconRect.width = 16.0f;
            iconRect.height = 16.0f;
            GUI.Box(iconRect, info.Icon, style.IconStyle);

            var firstColumnRect = new Rect(iconRect);
            firstColumnRect.x += iconRect.width;
            firstColumnRect.width = style.FirstColumnWidth;
            firstColumnRect.height = rowRect.height;
            if (style.FirstColumnWidth > 0)
            {
                var originalName = previewStepIndex >= 0 && previewStepIndex < info.RenameResultSequence.NumSteps ?
                    info.RenameResultSequence.GetOriginalNameAtStep(previewStepIndex, style.DeletionColor) :
                    info.RenameResultSequence.OriginalName;
                EditorGUI.LabelField(firstColumnRect, originalName, style.FirstColumnStyle);
            }

            var secondColumnRect = new Rect(firstColumnRect);
            secondColumnRect.x += firstColumnRect.width;
            secondColumnRect.width = style.SecondColumnWidth;
            secondColumnRect.height = rowRect.height;
            if (style.SecondColumnWidth > 0)
            {
                var newName = previewStepIndex >= 0 && previewStepIndex < info.RenameResultSequence.NumSteps ?
                    info.RenameResultSequence.GetNewNameAtStep(previewStepIndex, style.InsertionColor) :
                    info.RenameResultSequence.NewName;
                EditorGUI.LabelField(secondColumnRect, newName, style.SecondColumnStyle);
            }

            var thirdColumnRect = new Rect(secondColumnRect);
            thirdColumnRect.x += secondColumnRect.width;
            thirdColumnRect.width = style.ThirdColumnWidth;
            thirdColumnRect.height = rowRect.height;
            if (style.ThirdColumnWidth > 0)
            {
                EditorGUI.LabelField(thirdColumnRect, info.RenameResultSequence.NewName, style.ThirdColumnStyle);
            }

            return isDeleteClicked;
        }

        private void InitializeGUIContents()
        {
            this.guiContents = new GUIContents();

            this.guiContents.DropPrompt = new GUIContent(
                "No objects specified for rename. Drag objects here to rename them, or");

            this.guiContents.DropPromptHintInsideScroll = new GUIContent(
                "Add more objects by dragging them here");

            this.guiContents.DropPromptHint = new GUIContent(
                "Add more objects by dragging them into the above panel");

            this.guiContents.DropPromptRepeat = new GUIContent(
                "To rename more objects, drag them here, or");
        }

        private void InitializeGUIStyles()
        {
            this.guiStyles = new GUIStyles();

            this.guiStyles.Icon = GUIStyle.none;
            this.guiStyles.OriginalNameLabelUnModified = EditorStyles.label;
            this.guiStyles.OriginalNameLabelUnModified.richText = true;

            this.guiStyles.OriginalNameLabelWhenModified = EditorStyles.boldLabel;
            this.guiStyles.OriginalNameLabelWhenModified.richText = true;

            this.guiStyles.NewNameLabelUnModified = EditorStyles.label;
            this.guiStyles.NewNameLabelUnModified.richText = true;

            this.guiStyles.NewNameLabelModified = EditorStyles.boldLabel;
            this.guiStyles.NewNameLabelModified.richText = true;

            this.guiStyles.FinalNameLabelUnModified = EditorStyles.label;
            this.guiStyles.FinalNameLabelUnModified.richText = true;

            this.guiStyles.FinalNameLabelWhenModified = EditorStyles.boldLabel;
            this.guiStyles.FinalNameLabelWhenModified.richText = true;

            this.guiStyles.DropPrompt = new GUIStyle(EditorStyles.label);
            this.guiStyles.DropPrompt.alignment = TextAnchor.MiddleCenter;
            this.guiStyles.DropPrompt.wordWrap = true;
            this.guiStyles.DropPromptRepeat = new GUIStyle(EditorStyles.label);
            this.guiStyles.DropPromptRepeat.alignment = TextAnchor.MiddleCenter;

            this.guiStyles.DropPromptHintInsideScroll = EditorStyles.centeredGreyMiniLabel;
            this.guiStyles.DropPromptHint = EditorStyles.wordWrappedMiniLabel;

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
                this.guiStyles.PreviewScroll = EditorStyles.textArea;

                this.guiStyles.PreviewRowBackgroundEven = new Color(0.6f, 0.6f, 0.6f, 0.2f);

                this.guiStyles.InsertionTextColor = new Color32(0, 140, 104, 255);
                this.guiStyles.DeletionTextColor = new Color32(189, 47, 79, 255);
            }

            this.guiStyles.PreviewRowBackgroundOdd = Color.clear;
        }

        public Vector2 Draw(Rect previewPanelRect, Vector2 previewPanelScrollPosition, BulkRenamePreview preview)
        {
            var spaceBetweenFooterAndScrollview = 2.0f;
            var panelFooterToolbar = new Rect(previewPanelRect);
            panelFooterToolbar.height = EditorGUIUtility.singleLineHeight;
            panelFooterToolbar.y += (previewPanelRect.height + spaceBetweenFooterAndScrollview) - panelFooterToolbar.height;

            var scrollViewRect = new Rect(previewPanelRect);
            scrollViewRect.height -= (panelFooterToolbar.height + spaceBetweenFooterAndScrollview);

            GUI.Box(scrollViewRect, "", this.guiStyles.PreviewScroll);

            bool panelIsEmpty = preview.NumObjects == 0;
            var contentsFitWithoutScrolling = false;
            var newScrollPosition = Vector2.zero;
            if (panelIsEmpty)
            {
                this.DrawPreviewPanelContentsEmpty(scrollViewRect);
            }
            else
            {
                var previewContents = PreviewPanelContents.CreatePreviewContentsForObjects(preview);

                var scrollLayout = new PreviewPanelLayout(scrollViewRect);

                bool shouldShowSecondColumn = this.IsPreviewStepModePreference || this.NumRenameOperations == 1;
                bool shouldShowThirdColumn = !this.ShowPreviewSteps || this.NumRenameOperations > 1;
                var contentsLayout = new PreviewPanelContentsLayout(
                    scrollLayout.ScrollRect,
                    previewContents,
                    shouldShowSecondColumn,
                    shouldShowThirdColumn);

                newScrollPosition = this.DrawPreviewPanelContentsWithItems(
                    scrollLayout,
                    contentsLayout,
                    previewPanelScrollPosition,
                    previewContents,
                    shouldShowSecondColumn,
                    shouldShowThirdColumn);

                contentsFitWithoutScrolling = scrollLayout.ContentsFitWithoutAnyScrolling(contentsLayout);
            }

            var draggedObjects = this.GetDraggedObjectsOverRect(scrollViewRect);
            if (draggedObjects.Count > 0)
            {
                if (this.ObjectsDropped != null)
                {
                    this.ObjectsDropped.Invoke(draggedObjects.ToArray());
                }
            }

            if (!panelIsEmpty)
            {
                var buttonSpacing = 2.0f;
                var rightPadding = 2.0f;
                var addSelectedObjectsButtonRect = new Rect(panelFooterToolbar);
                addSelectedObjectsButtonRect.width = 150.0f;
                addSelectedObjectsButtonRect.x = panelFooterToolbar.xMax - rightPadding - addSelectedObjectsButtonRect.width;

                var removeAllButtonRect = new Rect(addSelectedObjectsButtonRect);
                removeAllButtonRect.width = 100.0f;
                removeAllButtonRect.x -= (removeAllButtonRect.width + buttonSpacing);
                if (GUI.Button(removeAllButtonRect, "Remove All"))
                {
                    if (this.RemoveAllClicked != null)
                    {
                        this.RemoveAllClicked.Invoke();
                    }
                }

                this.DrawAddSelectedObjectsButton(addSelectedObjectsButtonRect);

                if (!contentsFitWithoutScrolling)
                {
                    var hintRect = new Rect(scrollViewRect);
                    hintRect.height = EditorGUIUtility.singleLineHeight * 2.0f;
                    hintRect.y += scrollViewRect.height;
                    hintRect.width = scrollViewRect.width - addSelectedObjectsButtonRect.width - removeAllButtonRect.width - buttonSpacing;
                    EditorGUI.LabelField(hintRect, this.guiContents.DropPromptHint, this.guiStyles.DropPromptHint);
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
                    noun = "Object";
                }
                else
                {
                    noun = "Objects";
                }

                var renameSuccessContent = new GUIContent(string.Format("{0} {1} Renamed", this.NumPreviouslyRenamedObjects, noun));
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

            int renameStep = this.ShowPreviewSteps ? this.FocusedRenameOpIndex : -1;
            string originalNameColumnHeader = renameStep < 1 ? "Original" : "Before";
            string newNameColumnHeader = "After";
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

            // Show the one that doesn't quite fit by subtracting one
            var firstItemIndex = Mathf.Max(Mathf.FloorToInt(newScrollPosition.y / PreviewRowHeight) - 1, 0);

            // Add one for the one that's off screen.
            var numItems = Mathf.CeilToInt(scrollLayout.ScrollRect.height / PreviewRowHeight) + 1;

            var rowRect = new Rect(scrollLayout.ScrollRect);
            rowRect.width = Mathf.Max(contentsLayout.Rect.width, scrollLayout.ScrollRect.width);
            this.DrawPreviewRows(rowRect, renameStep, previewContents, firstItemIndex, numItems, shouldShowSecondColumn, shouldShowThirdColumn);

            // Add the hint into the scroll view if there's room
            if (scrollLayout.ContentsFitWithoutAnyScrolling(contentsLayout))
            {
                EditorGUI.LabelField(scrollLayout.HintRect, this.guiContents.DropPromptHintInsideScroll, this.guiStyles.DropPromptHintInsideScroll);
            }

            GUI.EndScrollView();

            // Put dividers in group so that they scroll (horizontally)
            GUI.BeginGroup(scrollLayout.ScrollRect);
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
            var dividerHeight = scrollLayout.ScrollRect.height - 1.0f;

            // Unity scroll view bars overlap the scroll rect when they render, so we have to shorten the dividers
            if (!scrollLayout.ContentsFitWithoutScrollingHorizontally(contentsLayout))
            {
                var scrollbarHeight = 14.0f;
                dividerHeight -= scrollbarHeight;
            }

            var firstDividerRect = new Rect(
                -newScrollPosition.x + contentsLayout.FirstColumnWidth + contentsLayout.IconSize,
                1.0f,
                1.0f,
                dividerHeight - 1.0f);
            GUI.DrawTexture(firstDividerRect, Texture2D.whiteTexture);

            if (shouldShowThirdColumn)
            {
                var secondDividerRect = new Rect(firstDividerRect);
                secondDividerRect.x += contentsLayout.SecondColumnWidth;
                GUI.DrawTexture(secondDividerRect, Texture2D.whiteTexture);
            }

            GUI.color = oldColor;
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
                EditorGUI.LabelField(thirdColumnRect, "Final Name", EditorStyles.boldLabel);
            }

            GUI.EndGroup();
        }

        private void DrawPreviewRows(Rect previewRowsRect, int stepIndex, PreviewPanelContents previewContents, int startingPreviewIndex, int numPreviewsToShow, bool showSecondColumn, bool showThirdColumn)
        {
            for (int i = startingPreviewIndex; i < startingPreviewIndex + numPreviewsToShow && i < previewContents.NumRows; ++i)
            {
                var content = previewContents[i];
                var previewRowStyle = new PreviewRowStyle();
                previewRowStyle.IconStyle = this.guiStyles.Icon;

                previewRowStyle.FirstColumnStyle = content.NamesAreDifferent ?
                    this.guiStyles.OriginalNameLabelWhenModified :
                    this.guiStyles.OriginalNameLabelUnModified;
                previewRowStyle.FirstColumnWidth = previewContents.LongestOriginalNameWidth;

                previewRowStyle.SecondColumnStyle = content.NamesAreDifferent ?
                    this.guiStyles.NewNameLabelModified :
                    this.guiStyles.NewNameLabelUnModified;

                previewRowStyle.SecondColumnWidth = showSecondColumn ? previewContents.LongestNewNameWidth : 0.0f;

                previewRowStyle.ThirdColumnStyle = content.NamesAreDifferent ?
                    this.guiStyles.FinalNameLabelWhenModified :
                    this.guiStyles.FinalNameLabelUnModified;

                previewRowStyle.ThirdColumnWidth = showThirdColumn ? previewContents.LongestFinalNameWidth : 0.0f;

                previewRowStyle.BackgroundColor = i % 2 == 0 ? this.guiStyles.PreviewRowBackgroundEven : this.guiStyles.PreviewRowBackgroundOdd;

                previewRowStyle.InsertionColor = this.guiStyles.InsertionTextColor;
                previewRowStyle.DeletionColor = this.guiStyles.DeletionTextColor;

                var rowRect = new Rect(previewRowsRect);
                rowRect.height = PreviewRowHeight;
                rowRect.y = previewRowsRect.y + (i * rowRect.height);
                if (DrawPreviewRow(rowRect, stepIndex, content, previewRowStyle))
                {
                    if (this.ObjectRemoved != null)
                    {
                        this.ObjectRemoved.Invoke(i);
                    }

                    break;
                }
            }
        }

        private void DrawAddSelectedObjectsButton(Rect buttonRect)
        {
            EditorGUI.BeginDisabledGroup(DisableAddSelectedObjectsButton);
            if (GUI.Button(buttonRect, "Add Selected Objects"))
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

            public PreviewPanelContentsLayout(Rect scrollRect, PreviewPanelContents previewContents, bool shouldShowSecondColumn, bool shouldShowThirdColumn)
            {
                this.FirstColumnWidth = previewContents.LongestOriginalNameWidth;
                this.SecondColumnWidth = shouldShowSecondColumn ? previewContents.LongestNewNameWidth : 0.0f;
                this.ThirdColumnWidth = shouldShowThirdColumn ? previewContents.LongestFinalNameWidth : 0.0f;

                var totalColumnWidth = this.FirstColumnWidth + this.SecondColumnWidth + this.ThirdColumnWidth;

                var rect = new Rect(scrollRect);
                rect.height = PreviewRowHeight * previewContents.NumRows;
                rect.width = totalColumnWidth + this.IconSize;
                this.Rect = rect;
            }
        }

        private class PreviewPanelContents
        {
            private const float MinColumnWidth = 150.0f;

            public float LongestOriginalNameWidth { get; private set; }

            public float LongestNewNameWidth { get; private set; }

            public float LongestFinalNameWidth { get; private set; }

            public int NumRows
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
                            "Trying to access PreviewRowModel at index that is out of bounds. Index: " + index);
                    }
                }
            }

            public static PreviewPanelContents CreatePreviewContentsForObjects(BulkRenamePreview preview)
            {
                var previewPanelContents = new PreviewPanelContents();
                previewPanelContents.PreviewRowInfos = new PreviewRowModel[preview.NumObjects];
                for (int i = 0; i < preview.NumObjects; ++i)
                {
                    var info = new PreviewRowModel();
                    var previewForIndex = preview.GetPreviewAtIndex(i);
                    info.RenameResultSequence = previewForIndex.RenameResultSequence;
                    info.Icon = previewForIndex.ObjectToRename.GetEditorIcon();

                    if (previewForIndex.HasWarnings)
                    {
                        info.WarningIcon = (Texture2D)EditorGUIUtility.Load("icons/console.warnicon.sml.png");
                        info.WarningMessage = previewForIndex.WarningMessage;
                    }
                    else
                    {
                        info.WarningIcon = null;
                        info.WarningMessage = string.Empty;
                    }

                    previewPanelContents.PreviewRowInfos[i] = info;
                }

                float paddingScaleForBold = 1.11f;
                previewPanelContents.LongestOriginalNameWidth = 0.0f;
                previewPanelContents.LongestNewNameWidth = 0.0f;
                foreach (var previewRowInfo in previewPanelContents.PreviewRowInfos)
                {
                    float originalNameWidth = GUI.skin.label.CalcSize(
                                                  new GUIContent(previewRowInfo.RenameResultSequence.OriginalName)).x * paddingScaleForBold;
                    if (originalNameWidth > previewPanelContents.LongestOriginalNameWidth)
                    {
                        previewPanelContents.LongestOriginalNameWidth = originalNameWidth;
                    }

                    float newNameWidth = GUI.skin.label.CalcSize(
                                             new GUIContent(previewRowInfo.RenameResultSequence.NewName)).x * paddingScaleForBold;
                    if (newNameWidth > previewPanelContents.LongestNewNameWidth)
                    {
                        previewPanelContents.LongestNewNameWidth = newNameWidth;
                    }
                }

                previewPanelContents.LongestOriginalNameWidth = Mathf.Max(MinColumnWidth, previewPanelContents.LongestOriginalNameWidth);
                previewPanelContents.LongestNewNameWidth = Mathf.Max(MinColumnWidth, previewPanelContents.LongestNewNameWidth);
                previewPanelContents.LongestFinalNameWidth = previewPanelContents.LongestNewNameWidth;

                return previewPanelContents;
            }
        }

        private struct PreviewRowModel
        {
            public Texture Icon { get; set; }

            public Texture WarningIcon { get; set; }

            public string WarningMessage { get; set; }

            public RenameResultSequence RenameResultSequence { get; set; }

            public bool NamesAreDifferent
            {
                get
                {
                    return this.RenameResultSequence.OriginalName != this.RenameResultSequence.NewName;
                }
            }
        }

        private struct PreviewRowStyle
        {
            public GUIStyle IconStyle { get; set; }

            public Color DeletionColor { get; set; }

            public Color InsertionColor { get; set; }

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