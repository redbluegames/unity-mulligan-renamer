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
        private const string VersionString = "1.3.0";
        private const string WindowMenuPath = "Window/Red Blue/Mulligan Renamer";

        private const string RenameOpsEditorPrefsKey = "RedBlueGames.MulliganRenamer.RenameOperationsToApply";
        private const string PreviewModePrefixKey = "RedBlueGames.MulliganRenamer.IsPreviewStepModePreference";

        private const float OperationPanelWidth = 350.0f;

        private GUIStyles guiStyles;
        private GUIContents guiContents;

        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;
        private MulliganRenamerPreviewPanel previewPanel;

        private int NumPreviouslyRenamedObjects { get; set; }

        private BulkRenamer BulkRenamer { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationDrawerBindingPrototypes { get; set; }

        private UniqueList<UnityEngine.Object> ObjectsToRename { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationsToApplyWithBindings { get; set; }

        private int NumRenameOperations
        {
            get
            {
                return this.RenameOperationsToApplyWithBindings.Count;
            }
        }

        private IRenameOperation OperationToForceFocus { get; set; }

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

        private IRenameOperation FocusedRenameOp
        {
            get
            {
                var focusedOpIndex = this.FocusedRenameOpIndex;
                if (focusedOpIndex >= 0 && focusedOpIndex < this.NumRenameOperations)
                {
                    return this.RenameOperationsToApplyWithBindings[this.FocusedRenameOpIndex].Operation;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool IsShowingPreviewSteps
        {
            get
            {
                // Show step previewing mode when only one operation is left because Results mode is pointless with one op only.
                // But don't actually change the mode preference so that adding ops restores whatever mode the user was in.
                return this.IsPreviewStepModePreference || this.NumRenameOperations <= 1;
            }
        }

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

        private static bool ObjectIsRenamable(UnityEngine.Object obj)
        {
            if (obj is GameObject)
            {
                return true;
            }

            if (AssetDatabase.Contains(obj))
            {
                // Create -> Prefab results in assets that have no name. Typically you can't have Assets that have no name,
                // so we will just ignore them for the utility.
                return !string.IsNullOrEmpty(obj.name);
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
                coloredHighlightRect.width += 1.0f;
                coloredHighlightRect.x += breacrumbConfig.UseLeftStyle ? -5.0f : -4.0f;
                var oldColor = GUI.color;
                GUI.color = breacrumbConfig.HighlightColor;
                GUI.DrawTexture(coloredHighlightRect, Texture2D.whiteTexture);
                GUI.color = oldColor;
            }

            return selected;
        }

        private void OnEnable()
        {
            AssetPreview.SetPreviewTextureCacheSize(100);
            this.minSize = new Vector2(600.0f, 300.0f);

            this.previewPanelScrollPosition = Vector2.zero;

            this.RenameOperationsToApplyWithBindings = new List<RenameOperationDrawerBinding>();
            this.ObjectsToRename = new UniqueList<UnityEngine.Object>();

            this.CacheRenameOperationPrototypes();
            this.LoadSavedRenameOperations();

            this.BulkRenamer = new BulkRenamer();
            Selection.selectionChanged += this.Repaint;
        }

        private void InitializePreviewPanel()
        {
            this.previewPanel = new MulliganRenamerPreviewPanel();
            this.previewPanel.ValidateObject = ObjectIsValidForRename;
            this.previewPanel.ObjectsDropped += this.HandleObjectsDroppedOverPreviewArea;
            this.previewPanel.RemoveAllClicked += this.HandleRemoveAllObjectsClicked;
            this.previewPanel.AddSelectedObjectsClicked += this.HandleAddSelectedObjectsClicked;
            this.previewPanel.ObjectRemoved += this.HandleObjectRemoved;
        }

        private void HandleObjectsDroppedOverPreviewArea(UnityEngine.Object[] objects)
        {
            this.AddObjectsToRename(objects);
            this.ScrollPreviewPanelToBottom();
        }

        private void HandleRemoveAllObjectsClicked()
        {
            this.ObjectsToRename.Clear();
        }

        private void HandleAddSelectedObjectsClicked()
        {
            this.LoadSelectedObjects();
        }

        private void HandleObjectRemoved(int index)
        {
            this.ObjectsToRename.RemoveAt(index);
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= this.Repaint;
        }

        private void CacheRenameOperationPrototypes()
        {
            // This binds Operations to their respective Drawers
            this.RenameOperationDrawerBindingPrototypes = new List<RenameOperationDrawerBinding>();
            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AddStringOperation(), new AddStringOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceStringOperation(), new ReplaceStringOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceNameOperation(), new ReplaceNameOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new EnumerateOperation(), new EnumerateOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ChangeCaseOperation(), new ChangeCaseOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new TrimCharactersOperation(), new TrimCharactersOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new RemoveCharactersOperation(), new RemoveCharactersOperationDrawer()));
        }

        private void InitializeGUIContents()
        {
            this.guiContents = new GUIContents();

            var copyrightLabel = string.Concat("Mulligan Renamer v", VersionString, ", ©2018 RedBlueGames");
            this.guiContents.CopyrightLabel = new GUIContent(copyrightLabel);

            var renameOpsLabel = new GUIContent("Rename Operations");
            this.guiContents.RenameOpsLabel = renameOpsLabel;
        }

        private void InitializeGUIStyles()
        {
            this.guiStyles = new GUIStyles();

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
                0.0f,
                0.0f,
                OperationPanelWidth,
                this.position.height - toolbarRect.height - footerHeight);
            this.DrawOperationsPanel(operationPanelRect);

            this.FocusForcedFocusControl();

            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            foreach (var binding in this.RenameOperationsToApplyWithBindings)
            {
                operationSequence.Add(binding.Operation);
            }

            this.BulkRenamer.SetRenameOperations(operationSequence);
            var bulkRenamePreview = this.BulkRenamer.GetBulkRenamePreview(this.ObjectsToRename.ToList());

            var previewPanelPadding = new RectOffset(1, 1, -1, 0);
            var previewPanelRect = new Rect(
                operationPanelRect.width + previewPanelPadding.left,
                toolbarRect.height + previewPanelPadding.top,
                this.position.width - operationPanelRect.width - previewPanelPadding.left - previewPanelPadding.right,
                this.position.height - toolbarRect.height - footerHeight - previewPanelPadding.top - previewPanelPadding.bottom);

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
                    this.NumPreviouslyRenamedObjects = this.BulkRenamer.RenameObjects(this.ObjectsToRename.ToList());
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
            var operationStyle = new GUIStyle("ScriptText");
            GUI.Box(toolbarRect, "", operationStyle);

            // The breadcrumb style spills to the left some so we need to claim extra space for it
            const float BreadcrumbLeftOffset = 7.0f;
            var breadcrumbRect = new Rect(
                new Vector2(BreadcrumbLeftOffset + OperationPanelWidth, toolbarRect.y + 1),
                new Vector2(toolbarRect.width - OperationPanelWidth - BreadcrumbLeftOffset, toolbarRect.height));

            this.DrawBreadcrumbs(this.IsShowingPreviewSteps, breadcrumbRect);

            EditorGUI.BeginDisabledGroup(this.NumRenameOperations <= 1);
            var buttonText = "Preview Steps";
            var previewButtonSize = new Vector2(100.0f, toolbarRect.height);
            var previewButtonPosition = new Vector2(toolbarRect.xMax - previewButtonSize.x, toolbarRect.y + 1);
            var toggleRect = new Rect(previewButtonPosition, previewButtonSize);
            this.IsPreviewStepModePreference = GUI.Toggle(toggleRect, this.IsPreviewStepModePreference, buttonText, "toolbarbutton");
            EditorGUI.EndDisabledGroup();
        }

        private void DrawBreadcrumbs(bool isShowingPreviewSteps, Rect rect)
        {
            if (this.NumRenameOperations == 0)
            {
                var emptyBreadcrumbRect = new Rect(rect);
                emptyBreadcrumbRect.width = 20.0f;
                EditorGUI.BeginDisabledGroup(true);
                GUI.Toggle(emptyBreadcrumbRect, false, string.Empty, "GUIEditor.BreadcrumbLeft");
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (isShowingPreviewSteps)
                {
                    var totalWidth = 0.0f;
                    for (int i = 0; i < this.NumRenameOperations; ++i)
                    {
                        var drawerAtBreadcrumb = this.RenameOperationsToApplyWithBindings[i].Drawer;
                        var breadcrumbOption = new PreviewBreadcrumbOptions();
                        breadcrumbOption.Heading = drawerAtBreadcrumb.HeadingLabel;
                        breadcrumbOption.HighlightColor = drawerAtBreadcrumb.HighlightColor;

                        breadcrumbOption.UseLeftStyle = i == 0;
                        breadcrumbOption.Enabled = i == this.FocusedRenameOpIndex;

                        var breadcrumbPosition = new Vector2(rect.x + totalWidth, rect.y);

                        var nextBreadcrumbRect = new Rect(breadcrumbPosition, breadcrumbOption.SizeForContent);
                        nextBreadcrumbRect.position = new Vector2(rect.x + totalWidth, rect.y);

                        var selected = DrawPreviewBreadcrumb(nextBreadcrumbRect, breadcrumbOption);
                        if (selected && i != this.FocusedRenameOpIndex)
                        {
                            var renameOp = this.RenameOperationsToApplyWithBindings[i].Operation;
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
        }

        private void DrawOperationsPanel(Rect operationPanelRect)
        {
            var totalHeightOfOperations = 0.0f;
            var operationSpacing = 0.0f;
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                var drawer = RenameOperationsToApplyWithBindings[i].Drawer;
                totalHeightOfOperations += drawer.GetPreferredHeight() + operationSpacing;
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
                foreach (var renameOpDrawerBindingPrototype in this.RenameOperationDrawerBindingPrototypes)
                {
                    var content = new GUIContent(renameOpDrawerBindingPrototype.Drawer.MenuDisplayPath);
                    menu.AddItem(content, false, this.OnAddRenameOperationConfirmed, renameOpDrawerBindingPrototype);
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
            IRenameOperation operationToFocus = null;

            var totalHeightDrawn = 0.0f;
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                var currentElement = this.RenameOperationsToApplyWithBindings[i];
                var rect = new Rect(operationRect);
                rect.y += totalHeightDrawn + spacing + headerRect.height;
                rect.height = currentElement.Drawer.GetPreferredHeight();
                totalHeightDrawn += rect.height + spacing;

                var guiOptions = new RenameOperationGUIOptions();
                guiOptions.ControlPrefix = i;
                guiOptions.DisableUpButton = i == 0;
                guiOptions.DisableDownButton = i == this.NumRenameOperations - 1;
                var buttonClickEvent = currentElement.Drawer.DrawGUI(rect, guiOptions);
                switch (buttonClickEvent)
                {
                    case RenameOperationSortingButtonEvent.MoveUp:
                        {
                            this.RenameOperationsToApplyWithBindings.MoveElementFromIndexToIndex(i, i - 1);
                            saveOpsToPreferences = true;

                            // Move focus with the RenameOp. This techincally changes their focus within the 
                            // rename op, but it's better than focus getting swapped to whatever op replaces this one.
                            operationToFocus = focusedOpBeforeButtonPresses;
                            break;
                        }

                    case RenameOperationSortingButtonEvent.MoveDown:
                        {
                            this.RenameOperationsToApplyWithBindings.MoveElementFromIndexToIndex(i, i + 1);
                            saveOpsToPreferences = true;
                            operationToFocus = focusedOpBeforeButtonPresses;
                            break;
                        }

                    case RenameOperationSortingButtonEvent.Delete:
                        {
                            var removingFocusedOperation = focusedOpBeforeButtonPresses == currentElement;

                            this.RenameOperationsToApplyWithBindings.RemoveAt(i);
                            saveOpsToPreferences = true;

                            if (removingFocusedOperation && this.NumRenameOperations > 0)
                            {
                                // Focus the RenameOp that took this one's place, if there is one. 
                                var indexToFocus = Mathf.Min(this.NumRenameOperations - 1, i);
                                operationToFocus = this.RenameOperationsToApplyWithBindings[indexToFocus].Operation;
                            }
                            else
                            {
                                operationToFocus = focusedOpBeforeButtonPresses;
                            }

                            break;
                        }

                    case RenameOperationSortingButtonEvent.None:
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

        private void OnAddRenameOperationConfirmed(object operation)
        {
            var operationDrawerBinding = operation as RenameOperationDrawerBinding;
            if (operationDrawerBinding == null)
            {
                throw new System.ArgumentException(
                    "MulliganRenamerWindow tried to add a new RenameOperation using a type that is not a subclass of RenameOperationDrawerBinding." +
                " Operation type: " +
                operation.GetType().ToString());
            }

            this.AddRenameOperation(operationDrawerBinding);
        }

        private void AddRenameOperation(RenameOperationDrawerBinding prototypeBinding)
        {
            // Reconstruct the operation and drawer so we are working with new instances
            var renameOp = prototypeBinding.Operation.Clone();
            var drawer = (IRenameOperationDrawer)System.Activator.CreateInstance(prototypeBinding.Drawer.GetType());
            drawer.SetModel(renameOp);

            var binding = new RenameOperationDrawerBinding(renameOp, drawer);
            this.RenameOperationsToApplyWithBindings.Add(binding);

            this.SaveRenameOperationsToPreferences();

            // Scroll to the bottom to focus the newly created operation.
            this.ScrollRenameOperationsToBottom();

            this.FocusRenameOperationDeferred(renameOp);
        }

        private void DrawPreviewPanel(Rect previewPanelRect, BulkRenamePreview bulkRenamePreview)
        {
            // PreviewPanel goes null when we recompile while the window is open
            if (this.previewPanel == null)
            {
                this.InitializePreviewPanel();
            }

            this.previewPanel.NumPreviouslyRenamedObjects = this.NumPreviouslyRenamedObjects;

            // If we aren't doing stepwise preview, send an invalid prefix so that the panel only renders before and after
            var previewIndex = this.IsShowingPreviewSteps ? this.FocusedRenameOpIndex : -1;
            this.previewPanel.PreviewStepIndexToShow = previewIndex;

            MulliganRenamerPreviewPanel.ColumnStyle columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.OriginalAndFinalOnly;
            if (this.NumRenameOperations <= 1)
            {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.StepwiseHideFinal;
            }
            else if (this.IsShowingPreviewSteps)
            {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.Stepwise;
            }
            else
            {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.OriginalAndFinalOnly;
            }

            this.previewPanel.ColumnsToShow = columnStyle;
            this.previewPanel.DisableAddSelectedObjectsButton = this.GetValidSelectedObjects().Count == 0;
            this.previewPanelScrollPosition = this.previewPanel.Draw(previewPanelRect, this.previewPanelScrollPosition, bulkRenamePreview);

        }

        private void SaveRenameOperationsToPreferences()
        {
            var allOpPathsCommaSeparated = string.Empty;
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                allOpPathsCommaSeparated += this.RenameOperationsToApplyWithBindings[i].Drawer.MenuDisplayPath;
                if (i != this.NumRenameOperations - 1)
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
                var operation = new ReplaceStringOperation();
                var drawer = new ReplaceStringOperationDrawer();
                drawer.SetModel(operation);
                var binding = new RenameOperationDrawerBinding(operation, drawer);
                this.RenameOperationsToApplyWithBindings.Add(binding);
            }
            else
            {
                var ops = serializedOps.Split(',');
                foreach (var op in ops)
                {
                    foreach (var binding in this.RenameOperationDrawerBindingPrototypes)
                    {
                        if (binding.Drawer.MenuDisplayPath == op)
                        {
                            this.AddRenameOperation(binding);
                            break;
                        }
                    }
                }
            }

            if (this.NumRenameOperations > 0)
            {
                this.FocusRenameOperationDeferred(this.RenameOperationsToApplyWithBindings.First().Operation);
            }
        }

        private void FocusRenameOperationDeferred(IRenameOperation renameOperation)
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
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                var binding = this.RenameOperationsToApplyWithBindings[i];
                if (binding.Operation == this.OperationToForceFocus)
                {
                    controlNameToForceFocus = GUIControlNameUtility.CreatePrefixedName(i, binding.Drawer.ControlToFocus);
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

        private void LoadSelectedObjects()
        {
            this.AddObjectsToRename(this.GetValidSelectedObjects());

            // Scroll to the bottom to focus the newly added objects.
            this.ScrollPreviewPanelToBottom();
        }

        private void AddObjectsToRename(ICollection<UnityEngine.Object> objectsToAdd)
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
            return Selection.objects.Where((obj) => ObjectIsValidForRename(obj)).ToList();
        }

        private bool ObjectIsValidForRename(UnityEngine.Object obj)
        {
            return ObjectIsRenamable(obj) && !this.ObjectsToRename.Contains(obj);
        }

        private bool RenameOperatationsHaveErrors()
        {
            foreach (var binding in this.RenameOperationsToApplyWithBindings)
            {
                if (binding.Operation.HasErrors())
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

        private class RenameOperationDrawerBinding
        {
            public IRenameOperation Operation { get; private set; }

            public IRenameOperationDrawer Drawer { get; private set; }

            public RenameOperationDrawerBinding(IRenameOperation operation, IRenameOperationDrawer drawer)
            {
                this.Operation = operation;
                this.Drawer = drawer;
            }
        }

        private class GUIStyles
        {
            public GUIStyle CopyrightLabel { get; set; }
        }

        private class GUIContents
        {
            public GUIContent CopyrightLabel { get; set; }

            public GUIContent RenameOpsLabel { get; set; }
        }
    }
}
