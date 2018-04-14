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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Tool that tries to allow renaming mulitple selections by parsing similar substrings
    /// </summary>
    public class MulliganRenamerWindow : EditorWindow
    {
        private const string VersionString = "1.2.1";
        private const string WindowMenuPath = "Window/Red Blue/Mulligan Renamer";

        private const string RenameOpsEditorPrefsKey = "RedBlueGames.MulliganRenamer.RenameOperationsToApply";
        private const string PreviewModePrefixKey = "RedBlueGames.MulliganRenamer.IsPreviewStepModePreference";

        private const float OperationPanelWidth = 350.0f;
        private const float PreviewPanelFirstColumnMinSize = 50.0f;
        private const float PreviewRowHeight = 18.0f;

        private GUIStyles guiStyles;
        private GUIContents guiContents;
        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;

        private int NumPreviouslyRenamedObjects { get; set; }

        private BulkRenamer BulkRenamer { get; set; }

        private List<RenameOperation> RenameOperationPrototypes { get; set; }

        private List<UnityEngine.Object> ObjectsToRename { get; set; }

        private RenameOperationSequence<RenameOperation> RenameOperationsToApply { get; set; }

        private RenameOperation OperationToForceFocus { get; set; }

        private int FocusedRenameOpIndex
        {
            get
            {
                var focusedControl = GUI.GetNameOfFocusedControl();
                if (string.IsNullOrEmpty(focusedControl))
                {
                    return -1;
                }

                return GUIControlNameUtility.GetPrefixFromName(focusedControl);
            }
        }

        private RenameOperation FocusedRenameOp
        {
            get
            {
                var focusedOpIndex = this.FocusedRenameOpIndex;
                if (focusedOpIndex >= 0 && focusedOpIndex < this.RenameOperationsToApply.Count)
                {
                    return this.RenameOperationsToApply[this.FocusedRenameOpIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        private bool IsShowingPreviewSteps { get; set; }

        private string LastFocusedControlName { get; set; }

        private bool IsPreviewStepModePreference
        {
            get
            {
                return EditorPrefs.GetBool(PreviewModePrefixKey, true);
            }

            set
            {
                EditorPrefs.SetBool(PreviewModePrefixKey, value);
            }
        }

        [MenuItem(WindowMenuPath, false)]
        private static void ShowRenameSpritesheetWindow()
        {
            var bulkRenamerWindow = EditorWindow.GetWindow<MulliganRenamerWindow>(true, "Mulligan Renamer", true);

            // When they launch via right click, we immediately load the objects in.
            bulkRenamerWindow.LoadSelectedObjects();
        }

        private static bool ObjectIsValidForRename(UnityEngine.Object obj)
        {
            if (AssetDatabase.Contains(obj))
            {
                // Create -> Prefab results in assets that have no name. Typically you can't have Assets that have no name,
                // so we will just ignore them for the utility.
                return !string.IsNullOrEmpty(obj.name);
            }

            if (obj is GameObject)
            {
                return true;
            }

            return false;
        }

        private static bool DrawPreviewBreadcrumb(Rect rect, PreviewBreadcrumbOptions breacrumbConfig)
        {
            var styleName = breacrumbConfig.StyleName;
            var enabled = breacrumbConfig.Enabled;
            bool selected = GUI.Toggle(rect, enabled, breacrumbConfig.Heading, styleName);
            if (selected)
            {
                var coloredHighlightRect = new Rect(rect);
                coloredHighlightRect.height = 2;
                coloredHighlightRect.x += -5.0f;
                var oldColor = GUI.color;
                GUI.color = breacrumbConfig.HighlightColor;
                GUI.DrawTexture(coloredHighlightRect, Texture2D.whiteTexture);
                GUI.color = oldColor;
            }

            return selected;
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

        private void OnEnable()
        {
            AssetPreview.SetPreviewTextureCacheSize(100);
            this.minSize = new Vector2(600.0f, 300.0f);

            this.previewPanelScrollPosition = Vector2.zero;

            this.RenameOperationsToApply = new RenameOperationSequence<RenameOperation>();
            this.ObjectsToRename = new List<UnityEngine.Object>();

            this.CacheRenameOperationPrototypes();
            this.LoadSavedRenameOperations();

            this.BulkRenamer = new BulkRenamer();
            Selection.selectionChanged += this.Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= this.Repaint;
        }

        private void CacheRenameOperationPrototypes()
        {
            this.RenameOperationPrototypes = new List<RenameOperation>();

            this.RenameOperationPrototypes.Add(new ReplaceStringOperation());
            this.RenameOperationPrototypes.Add(new ReplaceNameOperation());
            this.RenameOperationPrototypes.Add(new AddStringOperation());
            this.RenameOperationPrototypes.Add(new EnumerateOperation());
            this.RenameOperationPrototypes.Add(new TrimCharactersOperation());
            this.RenameOperationPrototypes.Add(new RemoveCharactersOperation());
            this.RenameOperationPrototypes.Add(new ChangeCaseOperation());
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

            var copyrightLabel = string.Concat("Mulligan Renamer v", VersionString, ", ©2018 RedBlueGames");
            this.guiContents.CopyrightLabel = new GUIContent(copyrightLabel);

            var renameOpsLabel = new GUIContent("Rename Operations");
            this.guiContents.RenameOpsLabel = renameOpsLabel;
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

            var copyrightStyle = new GUIStyle(EditorStyles.miniLabel);
            copyrightStyle.alignment = TextAnchor.MiddleRight;
            this.guiStyles.CopyrightLabel = copyrightStyle;
        }

        private void OnGUI()
        {
            // Initialize GUIContents and GUIStyles in OnGUI since it makes calls that must be done in OnGUI loop.
            if (this.guiContents == null)
            {
                this.InitializeGUIContents();
            }

            if (this.guiStyles == null)
            {
                this.InitializeGUIStyles();
            }

            // Remove any objects that got deleted while working
            this.ObjectsToRename.RemoveNullObjects();

            // Breadcrumbs take up more than a single line so we add a bit more
            var toolbarRect = new Rect(0.0f, 0.0f, this.position.width, EditorGUIUtility.singleLineHeight + 3.0f);
            this.DrawToolbar(toolbarRect);

            var footerHeight = 60.0f;
            var operationPanelRect = new Rect(
                1.0f,
                0.0f,
                OperationPanelWidth,
                this.position.height - toolbarRect.height - footerHeight);
            this.DrawOperationsPanel(operationPanelRect);

            this.FocusForcedFocusControl();

            // Generate preview contents
            var previewPanelPadding = new RectOffset(2, 1, 0, 0);
            var previewPanelRect = new Rect(
                operationPanelRect.width + previewPanelPadding.left,
                toolbarRect.height + previewPanelPadding.top,
                this.position.width - operationPanelRect.width - previewPanelPadding.left - previewPanelPadding.right,
                this.position.height - toolbarRect.height - footerHeight - previewPanelPadding.top - previewPanelPadding.bottom);
            this.BulkRenamer.SetRenameOperations(this.RenameOperationsToApply);
            var bulkRenamePreview = this.BulkRenamer.GetBulkRenamePreview(this.ObjectsToRename);
            this.DrawPreviewPanel(previewPanelRect, bulkRenamePreview);

            var disableRenameButton =
                this.RenameOperatationsHaveErrors() ||
                this.ObjectsToRename.Count == 0;
            EditorGUI.BeginDisabledGroup(disableRenameButton);
            var renameButtonPadding = new Vector4(30.0f, 16.0f, 30.0f, 16.0f);
            var renameButtonSize = new Vector2(this.position.width - renameButtonPadding.x - renameButtonPadding.z, 24.0f);
            var renameButtonRect = new Rect(
                renameButtonPadding.x,
                previewPanelRect.y + previewPanelRect.height + renameButtonPadding.y,
                renameButtonSize.x,
                renameButtonSize.y);

            if (GUI.Button(renameButtonRect, "Rename"))
            {
                var popupMessage = string.Concat(
                    "Some objects have warnings and will not be renamed. Do you want to rename the other objects in the group?");
                var skipWarning = !bulkRenamePreview.HasWarnings;
                if (skipWarning || EditorUtility.DisplayDialog("Warning", popupMessage, "Rename", "Cancel"))
                {
                    this.NumPreviouslyRenamedObjects = this.BulkRenamer.RenameObjects(this.ObjectsToRename);
                    this.ObjectsToRename.Clear();
                }

                // Opening the dialog breaks the layout stack, so ExitGUI to prevent a NullPtr.
                // https://answers.unity.com/questions/1353442/editorutilitysavefilepane-and-beginhorizontal-caus.html
                // NOTE: This may no longer be necessary after reworking the gui to use non-layout.
                EditorGUIUtility.ExitGUI();
            }

            EditorGUI.EndDisabledGroup();

            var copyrightRect = new Rect(
                0.0f,
                renameButtonRect.y + renameButtonRect.height + 2.0f,
                this.position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(copyrightRect, this.guiContents.CopyrightLabel, this.guiStyles.CopyrightLabel);

            // Issue #115 - Workaround to force focus to stay with whatever widget it was previously on...
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.IsNullOrEmpty(focusedControl))
            {
                GUI.FocusControl(this.LastFocusedControlName);
                EditorGUI.FocusTextInControl(this.LastFocusedControlName);
            }
            else
            {
                this.LastFocusedControlName = GUI.GetNameOfFocusedControl();
            }
        }

        private void DrawToolbar(Rect toolbarRect)
        {
            // The breadcrumb style spills to the left some so we need to claim extra space for it
            const float BreadcrumbLeftOffset = 8.0f;
            var breadcrumbRect = new Rect(
                new Vector2(BreadcrumbLeftOffset + OperationPanelWidth, toolbarRect.y),
                new Vector2(toolbarRect.width - OperationPanelWidth - BreadcrumbLeftOffset, toolbarRect.height));

            // Show step previewing mode when only one operation is left because Results mode is pointless with one op only.
            // But don't actually change the mode preference so that adding ops restores whatever mode the user was in.
            this.IsShowingPreviewSteps = this.IsPreviewStepModePreference || this.RenameOperationsToApply.Count <= 1;
            this.DrawBreadcrumbs(this.IsShowingPreviewSteps, breadcrumbRect);

            EditorGUI.BeginDisabledGroup(this.RenameOperationsToApply.Count <= 1);
            var buttonText = "Preview Steps";
            var previewButtonSize = new Vector2(100.0f, toolbarRect.height);
            var previewButtonPosition = new Vector2(toolbarRect.xMax - previewButtonSize.x, toolbarRect.y);
            var toggleRect = new Rect(previewButtonPosition, previewButtonSize);
            this.IsPreviewStepModePreference = GUI.Toggle(toggleRect, this.IsPreviewStepModePreference, buttonText, "toolbarbutton");
            EditorGUI.EndDisabledGroup();
        }

        private void DrawBreadcrumbs(bool isShowingPreviewSteps, Rect rect)
        {
            if (isShowingPreviewSteps)
            {
                var totalWidth = 0.0f;
                for (int i = 0; i < this.RenameOperationsToApply.Count; ++i)
                {
                    var operationAtBreadcrumb = this.RenameOperationsToApply[i];
                    var breadcrumbOption = new PreviewBreadcrumbOptions();
                    breadcrumbOption.Heading = operationAtBreadcrumb.HeadingLabel;
                    breadcrumbOption.HighlightColor = operationAtBreadcrumb.HighlightColor;

                    breadcrumbOption.UseLeftStyle = i == 0;
                    breadcrumbOption.Enabled = i == this.FocusedRenameOpIndex;

                    var breadcrumbPosition = new Vector2(rect.x + totalWidth, rect.y);

                    var nextBreadcrumbRect = new Rect(breadcrumbPosition, breadcrumbOption.SizeForContent);
                    nextBreadcrumbRect.position = new Vector2(rect.x + totalWidth, rect.y);

                    var selected = DrawPreviewBreadcrumb(nextBreadcrumbRect, breadcrumbOption);
                    if (selected && i != this.FocusedRenameOpIndex)
                    {
                        var renameOp = this.RenameOperationsToApply[i];
                        this.FocusRenameOperationDeferred(renameOp);
                    }

                    totalWidth += nextBreadcrumbRect.width;
                }
            }
            else
            {
                var breadcrumbeOptions =
                    new PreviewBreadcrumbOptions() { Heading = "Result", HighlightColor = Color.clear, Enabled = true, UseLeftStyle = true };
                var breadcrumbSize = breadcrumbeOptions.SizeForContent;
                breadcrumbSize.y = rect.height;
                DrawPreviewBreadcrumb(new Rect(rect.position, breadcrumbSize), breadcrumbeOptions);
            }
        }

        private void DrawOperationsPanel(Rect operationPanelRect)
        {
            var totalHeightOfOperations = 0.0f;
            var operationSpacing = 0.0f;
            for (int i = 0; i < this.RenameOperationsToApply.Count; ++i)
            {
                totalHeightOfOperations += this.RenameOperationsToApply[i].GetPreferredHeight() + operationSpacing;
            }

            var operationsContentsRect = new Rect(operationPanelRect);
            operationsContentsRect.height = totalHeightOfOperations + 18.0f;

            var buttonSize = new Vector2(150.0f, 20.0f);
            var spaceBetweenButton = 16.0f;
            var scrollContents = new Rect(operationsContentsRect);
            scrollContents.height += spaceBetweenButton + buttonSize.y;
            this.renameOperationsPanelScrollPosition = GUI.BeginScrollView(
                operationPanelRect,
                this.renameOperationsPanelScrollPosition,
                scrollContents);

            this.DrawRenameOperations(operationPanelRect, operationSpacing);

            var buttonRect = new Rect();
            buttonRect.x = operationsContentsRect.x + (operationsContentsRect.size.x / 2.0f) - (buttonSize.x / 2.0f);
            buttonRect.y = operationsContentsRect.y + operationsContentsRect.height + spaceBetweenButton;
            buttonRect.height = buttonSize.y;
            buttonRect.width = buttonSize.x;

            if (GUI.Button(buttonRect, "Add Operation"))
            {
                // Add enums to the menu
                var menu = new GenericMenu();
                foreach (var renameOp in this.RenameOperationPrototypes)
                {
                    var content = new GUIContent(renameOp.MenuDisplayPath);
                    menu.AddItem(content, false, this.OnAddRenameOperationConfirmed, renameOp);
                }

                menu.ShowAsContext();
            }

            GUI.EndScrollView();
        }

        private void DrawRenameOperations(Rect operationRect, float spacing)
        {
            var headerRect = new Rect(operationRect);
            headerRect.height = 18.0f;
            var operationStyle = new GUIStyle("ScriptText");
            GUI.Box(headerRect, "", operationStyle);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUI.LabelField(headerRect, "Rename Operations", headerStyle);

            // Store the op before buttons are pressed because buttons change focus
            var focusedOpBeforeButtonPresses = this.FocusedRenameOp;
            bool saveOpsToPreferences = false;
            RenameOperation operationToFocus = null;

            var totalHeightDrawn = 0.0f;
            for (int i = 0; i < this.RenameOperationsToApply.Count; ++i)
            {
                var currentElement = this.RenameOperationsToApply[i];
                var rect = new Rect(operationRect);
                rect.y += totalHeightDrawn + spacing + headerRect.height;
                rect.height = currentElement.GetPreferredHeight();
                totalHeightDrawn += rect.height + spacing;

                var guiOptions = new RenameOperation.GUIOptions();
                guiOptions.ControlPrefix = i;
                guiOptions.DisableUpButton = i == 0;
                guiOptions.DisableDownButton = i == this.RenameOperationsToApply.Count - 1;
                var buttonClickEvent = currentElement.DrawGUI(rect, guiOptions);
                switch (buttonClickEvent)
                {
                    case RenameOperation.ListButtonEvent.MoveUp:
                        {
                            this.RenameOperationsToApply.MoveOperationFromIndexToIndex(i, i - 1);
                            saveOpsToPreferences = true;

                            // Move focus with the RenameOp. This techincally changes their focus within the 
                            // rename op, but it's better than focus getting swapped to whatever op replaces this one.
                            operationToFocus = focusedOpBeforeButtonPresses;
                            break;
                        }

                    case RenameOperation.ListButtonEvent.MoveDown:
                        {
                            this.RenameOperationsToApply.MoveOperationFromIndexToIndex(i, i + 1);
                            saveOpsToPreferences = true;
                            operationToFocus = focusedOpBeforeButtonPresses;
                            break;
                        }

                    case RenameOperation.ListButtonEvent.Delete:
                        {
                            var removingFocusedOperation = focusedOpBeforeButtonPresses == currentElement;

                            this.RenameOperationsToApply.RemoveAt(i);
                            saveOpsToPreferences = true;

                            if (removingFocusedOperation && this.RenameOperationsToApply.Count > 0)
                            {
                                // Focus the RenameOp that took this one's place, if there is one. 
                                var indexToFocus = Mathf.Min(this.RenameOperationsToApply.Count - 1, i);
                                operationToFocus = this.RenameOperationsToApply[indexToFocus];
                            }
                            else
                            {
                                operationToFocus = focusedOpBeforeButtonPresses;
                            }

                            break;
                        }

                    case RenameOperation.ListButtonEvent.None:
                        {
                            // Do nothing
                            break;
                        }

                    default:
                        {
                            Debug.LogError(string.Format(
                                    "RenamerWindow found Unrecognized ListButtonEvent [{0}] in OnGUI. Add a case to handle this event.",
                                    buttonClickEvent));
                            return;
                        }
                }

                if (operationToFocus != null)
                {
                    this.FocusRenameOperationDeferred(operationToFocus);
                }

                if (saveOpsToPreferences)
                {
                    this.SaveRenameOperationsToPreferences();
                }
            }
        }

        private void DrawPreviewPanel(Rect previewPanelRect, BulkRenamePreview preview)
        {
            var spaceBetweenFooterAndScrollview = 2.0f;
            var panelFooterToolbar = new Rect(previewPanelRect);
            panelFooterToolbar.height = EditorGUIUtility.singleLineHeight;
            panelFooterToolbar.y += (previewPanelRect.height + spaceBetweenFooterAndScrollview) - panelFooterToolbar.height;

            var scrollViewRect = new Rect(previewPanelRect);
            scrollViewRect.height -= (panelFooterToolbar.height + spaceBetweenFooterAndScrollview);

            GUI.Box(scrollViewRect, "", this.guiStyles.PreviewScroll);

            bool panelIsEmpty = this.ObjectsToRename.Count == 0;
            var contentsFitWithoutScrolling = false;
            if (panelIsEmpty)
            {
                this.DrawPreviewPanelContentsEmpty(scrollViewRect);
            }
            else
            {
                var previewContents = PreviewPanelContents.CreatePreviewContentsForObjects(preview);
                contentsFitWithoutScrolling = this.DrawPreviewPanelContentsWithItems(scrollViewRect, previewContents);
            }

            var draggedObjects = this.GetDraggedObjectsOverRect(scrollViewRect);
            if (draggedObjects.Count > 0)
            {
                this.AddObjectsToRename(draggedObjects);
                this.ScrollPreviewPanelToBottom();
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
                    this.ObjectsToRename.Clear();
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

        private bool DrawPreviewPanelContentsWithItems(Rect previewPanelRect, PreviewPanelContents previewContents)
        {
            var spaceBetweenHeaderAndScroll = 1.0f;
            var headerRect = new Rect(previewPanelRect);
            headerRect.height = 18.0f;

            var scrollRect = new Rect(previewPanelRect);
            scrollRect.height -= (headerRect.height + spaceBetweenHeaderAndScroll);
            scrollRect.y += (headerRect.height + spaceBetweenHeaderAndScroll);

            int renameStep = this.IsShowingPreviewSteps ? this.FocusedRenameOpIndex : -1;
            string originalNameColumnHeader = renameStep < 1 ? "Original" : "Before";
            string newNameColumnHeader = "After";
            bool shouldShowSecondColumn = this.IsPreviewStepModePreference || this.RenameOperationsToApply.Count == 1;
            bool shouldShowThirdColumn = !this.IsShowingPreviewSteps || this.RenameOperationsToApply.Count > 1;
            var firstColumnWidth = previewContents.LongestOriginalNameWidth;
            var secondColumnWidth = shouldShowSecondColumn ? previewContents.LongestNewNameWidth : 0.0f;
            var thirdColumnWidth = shouldShowThirdColumn ? previewContents.LongestFinalNameWidth : 0.0f;
            var totalColumnWidth = firstColumnWidth + secondColumnWidth + thirdColumnWidth;

            var scrollContentsRect = new Rect(scrollRect);
            scrollContentsRect.height = PreviewRowHeight * previewContents.NumRows;

            var hackSizeForRowIcons = 48.0f;
            scrollContentsRect.width = totalColumnWidth + hackSizeForRowIcons;

            this.DrawPreviewHeader(
                headerRect,
                -this.previewPanelScrollPosition.x,
                originalNameColumnHeader,
                newNameColumnHeader,
                firstColumnWidth,
                secondColumnWidth,
                thirdColumnWidth);

            this.previewPanelScrollPosition = GUI.BeginScrollView(
                scrollRect,
                this.previewPanelScrollPosition,
                scrollContentsRect);

            // Show the one that doesn't quite fit by subtracting one
            var firstItemIndex = Mathf.Max(Mathf.FloorToInt(this.previewPanelScrollPosition.y / PreviewRowHeight) - 1, 0);

            // Add one for the one that's off screen.
            var numItems = Mathf.CeilToInt(scrollRect.height / PreviewRowHeight) + 1;

            var rowRect = new Rect(scrollRect);
            rowRect.width = Mathf.Max(scrollContentsRect.width, scrollRect.width);
            this.DrawPreviewRows(rowRect, renameStep, previewContents, firstItemIndex, numItems, shouldShowSecondColumn, shouldShowThirdColumn);

            // Add the hint into the scroll view if there's room
            var hintRect = new Rect(scrollRect);
            hintRect.height = EditorGUIUtility.singleLineHeight;
            var contentsFitWithoutScrolling = scrollContentsRect.height + hintRect.height <= scrollRect.height &&
                                                                scrollContentsRect.width <= scrollRect.width;
            if (contentsFitWithoutScrolling)
            {
                hintRect.y += scrollRect.height - hintRect.height;
                EditorGUI.LabelField(hintRect, this.guiContents.DropPromptHintInsideScroll, this.guiStyles.DropPromptHintInsideScroll);
            }

            GUI.EndScrollView();

            return contentsFitWithoutScrolling;
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
                    this.ObjectsToRename.Remove(this.ObjectsToRename[i]);
                    break;
                }
            }
        }

        private void DrawAddSelectedObjectsButton(Rect buttonRect)
        {
            var newlySelectedObjects = this.GetValidSelectedObjects();
            EditorGUI.BeginDisabledGroup(newlySelectedObjects.Count == 0);
            if (GUI.Button(buttonRect, "Add Selected Objects"))
            {
                this.LoadSelectedObjects();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnAddRenameOperationConfirmed(object operation)
        {
            var operationAsRenameOp = operation as RenameOperation;
            if (operationAsRenameOp == null)
            {
                throw new System.ArgumentException(
                    "MulliganRenamerWindow tried to add a new RenameOperation using a type that is not a subclass of BaseRenameOperation." +
                    " Operation type: " +
                    operation.GetType().ToString());
            }

            this.AddRenameOperation(operationAsRenameOp);
        }

        private void AddRenameOperation(RenameOperation operation)
        {
            // Construct the Rename op
            var renameOp = operation.Clone();
            this.RenameOperationsToApply.Add(renameOp);

            this.SaveRenameOperationsToPreferences();

            // Scroll to the bottom to focus the newly created operation.
            this.ScrollRenameOperationsToBottom();

            this.FocusRenameOperationDeferred(renameOp);
        }

        private void SaveRenameOperationsToPreferences()
        {
            var allOpPathsCommaSeparated = string.Empty;
            for (int i = 0; i < this.RenameOperationsToApply.Count; ++i)
            {
                allOpPathsCommaSeparated += this.RenameOperationsToApply[i].MenuDisplayPath;
                if (i != this.RenameOperationsToApply.Count - 1)
                {
                    allOpPathsCommaSeparated += ",";
                }
            }

            EditorPrefs.SetString(RenameOpsEditorPrefsKey, allOpPathsCommaSeparated);
        }

        private void LoadSavedRenameOperations()
        {
            var serializedOps = EditorPrefs.GetString(RenameOpsEditorPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(serializedOps))
            {
                this.RenameOperationsToApply.Add(new ReplaceStringOperation());
            }
            else
            {
                var ops = serializedOps.Split(',');
                foreach (var op in ops)
                {
                    foreach (var prototypeOp in this.RenameOperationPrototypes)
                    {
                        if (prototypeOp.MenuDisplayPath == op)
                        {
                            this.AddRenameOperation(prototypeOp);
                            break;
                        }
                    }
                }
            }

            if (this.RenameOperationsToApply.Count > 0)
            {
                this.FocusRenameOperationDeferred(this.RenameOperationsToApply.First());
            }
        }

        private void FocusRenameOperationDeferred(RenameOperation renameOperation)
        {
            this.OperationToForceFocus = renameOperation;
        }

        private void FocusForcedFocusControl()
        {
            if (this.OperationToForceFocus == null)
            {
                return;
            }

            var controlNameToForceFocus = string.Empty;
            for (int i = 0; i < this.RenameOperationsToApply.Count; ++i)
            {
                var renameOp = this.RenameOperationsToApply[i];
                if (renameOp == this.OperationToForceFocus)
                {
                    controlNameToForceFocus = GUIControlNameUtility.CreatePrefixedName(i, renameOp.ControlToFocus);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(controlNameToForceFocus))
            {
                var previouslyFocusedControl = GUI.GetNameOfFocusedControl();

                // Try to focus the desired control
                GUI.FocusControl(controlNameToForceFocus);
                EditorGUI.FocusTextInControl(controlNameToForceFocus);

                // Stop focusing the desired control only once it's been focused.
                // (Workaround because for some reason this fails to focus a control when users click between breadcrumbs)
                var focusedControl = GUI.GetNameOfFocusedControl();
                if (controlNameToForceFocus.Equals(focusedControl))
                {
                    this.FocusRenameOperationDeferred(null);
                }
                else
                {
                    // If we weren't able to focus the new control, go back to whatever was focused before.
                    GUI.FocusControl(previouslyFocusedControl);
                    EditorGUI.FocusTextInControl(previouslyFocusedControl);
                }
            }
        }

        private List<UnityEngine.Object> GetDraggedObjectsOverRect(Rect dropArea)
        {
            Event currentEvent = Event.current;

            var droppedObjects = new List<UnityEngine.Object>();
            if (!dropArea.Contains(currentEvent.mousePosition))
            {
                return droppedObjects;
            }

            var validDraggedObjects = this.GetValidObjectsForRenameFromGroup(DragAndDrop.objectReferences);
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

        private void LoadSelectedObjects()
        {
            this.AddObjectsToRename(this.GetValidSelectedObjects());

            // Scroll to the bottom to focus the newly added objects.
            this.ScrollPreviewPanelToBottom();
        }

        private void AddObjectsToRename(List<UnityEngine.Object> objectsToAdd)
        {
            // Sort the objects before adding them
            var assets = new List<UnityEngine.Object>();
            var gameObjects = new List<UnityEngine.Object>();
            foreach (var obj in objectsToAdd)
            {
                if (obj.IsAsset())
                {
                    assets.Add(obj);
                }
                else
                {
                    gameObjects.Add((GameObject)obj);
                }
            }

            // When clicking and dragging from the scene, GameObjects are properly sorted according to the hierarchy.
            // But when selected and adding them, they are not. So we need to resort them here.
            gameObjects.Sort((x, y) => ((GameObject)x).GetHierarchySorting().CompareTo(((GameObject)y).GetHierarchySorting()));

            assets.Sort((x, y) =>
                {
                    return EditorUtility.NaturalCompare(x.name, y.name);
                });

            this.ObjectsToRename.AddRange(assets);
            this.ObjectsToRename.AddRange(gameObjects);

            // Reset the number of previously renamed objects so that we don't show the success prompt if these are removed.
            this.NumPreviouslyRenamedObjects = 0;
        }

        private List<UnityEngine.Object> GetValidSelectedObjects()
        {
            return this.GetValidObjectsForRenameFromGroup(Selection.objects);
        }

        private List<UnityEngine.Object> GetValidObjectsForRenameFromGroup(ICollection<UnityEngine.Object> objects)
        {
            var validObjects = new List<UnityEngine.Object>();
            foreach (var selectedObject in objects)
            {
                if (ObjectIsValidForRename(selectedObject) && !this.ObjectsToRename.Contains(selectedObject))
                {
                    validObjects.Add(selectedObject);
                }
            }

            return validObjects;
        }

        private bool RenameOperatationsHaveErrors()
        {
            foreach (var renameOperation in this.RenameOperationsToApply)
            {
                if (renameOperation.HasErrors)
                {
                    return true;
                }
            }

            return false;
        }

        private void ScrollPreviewPanelToBottom()
        {
            this.previewPanelScrollPosition = new Vector2(0.0f, 100000);
        }

        private void ScrollRenameOperationsToBottom()
        {
            this.renameOperationsPanelScrollPosition = new Vector2(0.0f, 100000);
        }

        private struct PreviewBreadcrumbOptions
        {
            public string Heading { get; set; }

            public Color32 HighlightColor { get; set; }

            public bool UseLeftStyle { get; set; }

            public bool Enabled { get; set; }

            public string StyleName
            {
                get
                {
                    return this.UseLeftStyle ? "GUIEditor.BreadcrumbLeft" : "GUIEditor.BreadcrumbMid";
                }
            }

            public Vector2 SizeForContent
            {
                get
                {
                    var style = new GUIStyle(this.StyleName);
                    return style.CalcSize(new GUIContent(this.Heading, string.Empty));
                }
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

            public GUIStyle CopyrightLabel { get; set; }
        }

        private class GUIContents
        {
            public GUIContent DropPrompt { get; set; }

            public GUIContent DropPromptRepeat { get; set; }

            public GUIContent DropPromptHint { get; set; }

            public GUIContent DropPromptHintInsideScroll { get; set; }

            public GUIContent CopyrightLabel { get; set; }

            public GUIContent RenameOpsLabel { get; set; }
        }
    }
}
