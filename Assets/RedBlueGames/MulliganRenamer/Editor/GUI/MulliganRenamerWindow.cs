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
    public class MulliganRenamerWindow : EditorWindow, IHasCustomMenu
    {
        private const string VersionString = "1.7.8";
        private const string WindowMenuPath = "Window/Red Blue/Mulligan Renamer";

        private const string RenameOpsEditorPrefsKey = "RedBlueGames.MulliganRenamer.RenameOperationsToApply";
        private const string PreviewModePrefixKey = "RedBlueGames.MulliganRenamer.IsPreviewStepModePreference";

        private const float OperationPanelWidth = 350.0f;

        private GUIStyles guiStyles;
        private GUIContents guiContents;

        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;
        private MulliganRenamerPreviewPanel previewPanel;
        private SavePresetWindow activeSavePresetWindow;
        private ManagePresetsWindow activePresetManagementWindow;

        private int NumPreviouslyRenamedObjects { get; set; }

        private BulkRenamer BulkRenamer { get; set; }

        private BulkRenamePreview BulkRenamePreview { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationDrawerBindingPrototypes { get; set; }

        private UniqueList<UnityEngine.Object> ObjectsToRename { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationsToApplyWithBindings { get; set; }

        private MulliganUserPreferences ActivePreferences { get; set; }

        private string CurrentPresetName { get; set; }

        private bool IsNewSession { get; set; }

        private bool IsShowingThanksForReview { get; set; }

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

                if (!GUIControlNameUtility.IsControlNamePrefixed(focusedControl))
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

        private List<UnityEngine.Object> ValidSelectedObjects { get; set; }

        private bool NeedsReview
        {
            get
            {
                return this.ActivePreferences.NeedsReview || this.IsShowingThanksForReview;
            }
        }

        [MenuItem(WindowMenuPath, false)]
        public static MulliganRenamerWindow ShowWindow()
        {
            return EditorWindow.GetWindow<MulliganRenamerWindow>(false, "Mulligan Renamer", true);
        }

        public static void ShowWindowWithSelectedObjects()
        {
            var bulkRenamerWindow = ShowWindow();
            bulkRenamerWindow.LoadSelectedObjects();

            // Fix Issue #235. RenamePreview is cached OnEnable and Application Update,
            // so when GUI draws this frame there's actually no preview, yet, so the 
            // "Add Selected Objects" (Empty) preview panel flickers for a frame.
            bulkRenamerWindow.CacheBulkRenamerPreview();
            bulkRenamerWindow.CacheValidSelectedObjects();
        }

        private static bool ObjectIsRenamable(UnityEngine.Object obj)
        {
            // Workaround for Issue #200 where AssetDatabase call during EditorApplicationUpdate caused a Null Reference Exception
            bool objectIsAsset = false;
            try
            {
                objectIsAsset = AssetDatabase.Contains(obj);
            }
            catch (System.NullReferenceException)
            {
                // Can't access the AssetDatabase at this time.
                return false;
            }

            if (objectIsAsset)
            {
                // Only sub assets of sprites are currently supported, so let's just not let them be added.
                if (AssetDatabase.IsSubAsset(obj) && !(obj is Sprite))
                {
                    return false;
                }

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
                coloredHighlightRect.width += 1.0f;
                coloredHighlightRect.x += breacrumbConfig.UseLeftStyle ? -5.0f : -4.0f;
                var oldColor = GUI.color;
                GUI.color = breacrumbConfig.HighlightColor;
                GUI.DrawTexture(coloredHighlightRect, Texture2D.whiteTexture);
                GUI.color = oldColor;
            }

            return selected;
        }

        /// <summary>
        /// Add the menu items for the generic Context Menu
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(
                new GUIContent(
                    LocalizationManager.Instance.GetTranslation("preferences")),
                    false,
                    () => this.ShowPreferencesWindowForCurrentUnityVersion());
        }

        private void OnEnable()
        {
            AssetPreview.SetPreviewTextureCacheSize(100);
            this.minSize = new Vector2(600.0f, 300.0f);

            this.previewPanelScrollPosition = Vector2.zero;

            this.RenameOperationsToApplyWithBindings = new List<RenameOperationDrawerBinding>();
            this.ObjectsToRename = new UniqueList<UnityEngine.Object>();

            this.CacheRenameOperationPrototypes();

            this.CurrentPresetName = string.Empty;
            this.LoadUserPreferences();

            // Intentionally forget their last preset when opening the window, because the user won't
            // remember they previously loaded a preset. It will only confuse them if the Save As
            // is populated with this name.
            this.CurrentPresetName = string.Empty;

            this.IsNewSession = true;
            this.IsShowingThanksForReview = false;

            this.BulkRenamer = new BulkRenamer();
            Selection.selectionChanged += this.Repaint;

            EditorApplication.update += this.CacheBulkRenamerPreview;
            EditorApplication.update += this.CacheValidSelectedObjects;

            LocalizationManager.Instance.LanguageChanged += this.HandleLanguageChanged;

            // Sometimes, GUI happens before Editor Update, so also cache a preview now.
            this.CacheBulkRenamerPreview();
            this.CacheValidSelectedObjects();
        }

        private void CacheBulkRenamerPreview()
        {
            var operationSequence = this.GetCurrentRenameOperationSequence();
            this.BulkRenamer.SetRenameOperations(operationSequence);
            this.BulkRenamePreview = this.BulkRenamer.GetBulkRenamePreview(this.ObjectsToRename.ToList());
        }

        private void CacheValidSelectedObjects()
        {
            this.ValidSelectedObjects = Selection.objects.Where((obj) => ObjectIsValidForRename(obj)).ToList();
        }

        private void InitializePreviewPanel()
        {
            this.previewPanel = new MulliganRenamerPreviewPanel();
            this.previewPanel.ValidateObject = ObjectIsValidForRename;
            this.previewPanel.ObjectsDropped += this.HandleObjectsDroppedOverPreviewArea;
            this.previewPanel.RemoveAllClicked += this.HandleRemoveAllObjectsClicked;
            this.previewPanel.AddSelectedObjectsClicked += this.HandleAddSelectedObjectsClicked;
            this.previewPanel.ObjectRemovedAtIndex += this.HandleObjectRemoved;
            this.previewPanel.ColumnsResized += this.Repaint;
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

        private void HandleLanguageChanged()
        {
            if (this.previewPanel != null)
            {
                this.previewPanel.RefreshGUIContent();
            }
        }

        private void OnDisable()
        {
            this.SaveUserPreferences();

            // If they've opened up the save preset window and are closing mulligan window, close the save preset
            // window because it can cause bugs since it can still invoke callbacks.
            // Same for presets window.
            if (this.activeSavePresetWindow != null)
            {
                this.activeSavePresetWindow.Close();
            }

            if (this.activePresetManagementWindow != null)
            {
                this.activePresetManagementWindow.Close();
            }

            Selection.selectionChanged -= this.Repaint;
            EditorApplication.update -= this.CacheBulkRenamerPreview;
            EditorApplication.update -= this.CacheValidSelectedObjects;

            LocalizationManager.Instance.LanguageChanged -= this.HandleLanguageChanged;
        }

        private void CacheRenameOperationPrototypes()
        {
            // This binds Operations to their respective Drawers
            this.RenameOperationDrawerBindingPrototypes = new List<RenameOperationDrawerBinding>();
            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceStringOperation(), new ReplaceStringOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceNameOperation(), new ReplaceNameOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AddStringOperation(), new AddStringOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new EnumerateOperation(), new EnumerateOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new CountByLetterOperation(), new CountByLetterOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AddStringSequenceOperation(), new AddStringSequenceOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ChangeCaseOperation(), new ChangeCaseOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ToCamelCaseOperation(), new ToCamelCaseOperationDrawer()));

            this.RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AdjustNumberingOperation(), new AdjustNumberingOperationDrawer()));

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

            var reviewPromptHeight = 0.0f;
            if (this.NeedsReview)
            {
                // Responsiveness: Expand height as the window shrinks to better fit the text
                if (this.position.width > 800.0f)
                {
                    reviewPromptHeight = 38.0f;
                }
                else
                {
                    reviewPromptHeight = 48.0f;
                }
            }
            var reviewPromptPaddingY = 16.0f;
            var footerHeight = 60.0f + reviewPromptHeight + reviewPromptPaddingY;
            var operationPanelRect = new Rect(
                0.0f,
                0.0f,
                OperationPanelWidth,
                this.position.height - toolbarRect.height - footerHeight);
            this.DrawOperationsPanel(operationPanelRect);

            var previewPanelPadding = new RectOffset(1, 1, -1, 0);
            var previewPanelRect = new Rect(
                operationPanelRect.width + previewPanelPadding.left,
                toolbarRect.height + previewPanelPadding.top,
                this.position.width - operationPanelRect.width - previewPanelPadding.left - previewPanelPadding.right,
                this.position.height - toolbarRect.height - footerHeight - previewPanelPadding.top - previewPanelPadding.bottom);

            this.DrawPreviewPanel(previewPanelRect, this.BulkRenamePreview);

            var rectForReviewWidth = this.position.width * 0.98f;
            var rectForReviewPrompt = new Rect(
                (this.position.width - rectForReviewWidth) * 0.5f,
                previewPanelRect.y + previewPanelRect.height + reviewPromptPaddingY,
                rectForReviewWidth,
                reviewPromptHeight);

            if (this.NeedsReview)
            {
                this.DrawReviewPrompt(rectForReviewPrompt);
            }

            var disableRenameButton =
                this.RenameOperatationsHaveErrors() ||
                this.ObjectsToRename.Count == 0;
            EditorGUI.BeginDisabledGroup(disableRenameButton);
            var renameButtonPadding = new Vector4(30.0f, 16.0f, 30.0f, 16.0f);
            var renameButtonSize = new Vector2(this.position.width - renameButtonPadding.x - renameButtonPadding.z, 24.0f);
            var renameButtonRect = new Rect(
                renameButtonPadding.x,
                rectForReviewPrompt.y + rectForReviewPrompt.height + renameButtonPadding.y,
                renameButtonSize.x,
                renameButtonSize.y);

            if (GUI.Button(renameButtonRect, LocalizationManager.Instance.GetTranslation("rename")))
            {
                var popupMessage = string.Concat(
                    LocalizationManager.Instance.GetTranslation("renameWarningNotRenamed"));
                var renamesHaveNoWarnings = !BulkRenamePreview.HasWarnings;
                if (renamesHaveNoWarnings || EditorUtility.DisplayDialog(LocalizationManager.Instance.GetTranslation("warning"),
                                                                            popupMessage,
                                                                            LocalizationManager.Instance.GetTranslation("rename"),
                                                                            LocalizationManager.Instance.GetTranslation("cancel")))
                {
                    var undoGroupBeforeRename = Undo.GetCurrentGroup();
                    try
                    {
                        this.NumPreviouslyRenamedObjects = this.BulkRenamer.RenameObjects(this.ObjectsToRename.ToList());
                        this.ObjectsToRename.Clear();
                        if (this.IsNewSession)
                        {
                            this.ActivePreferences.NumSessionsUsed++;
                            this.IsNewSession = false;
                        }
                    }
                    catch (System.OperationCanceledException e)
                    {
                        var errorMessage = string.Concat(
                            LocalizationManager.Instance.GetTranslation("failToRenameMulligan"),
                            e.Message);
                        if (EditorUtility.DisplayDialog(LocalizationManager.Instance.GetTranslation("error"), errorMessage, "Ok"))
                        {
                            Undo.RevertAllDownToGroup(undoGroupBeforeRename);
                        }

                        EditorGUIUtility.ExitGUI();
                    }
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

            this.FocusForcedFocusControl();
        }

        private void DrawToolbar(Rect toolbarRect)
        {
            var operationStyle = new GUIStyle(EditorStyles.toolbar);
            GUI.Box(toolbarRect, "", operationStyle);

            // The breadcrumb style spills to the left some so we need to claim extra space for it
            const float BreadcrumbLeftOffset = 7.0f;
            var breadcrumbRect = new Rect(
                new Vector2(BreadcrumbLeftOffset + OperationPanelWidth, toolbarRect.y),
                new Vector2(toolbarRect.width - OperationPanelWidth - BreadcrumbLeftOffset, toolbarRect.height));

            this.DrawBreadcrumbs(this.IsShowingPreviewSteps, breadcrumbRect);

            EditorGUI.BeginDisabledGroup(this.NumRenameOperations <= 1);
            var buttonText = LocalizationManager.Instance.GetTranslation("previewSteps");
            var previewButtonSize = new Vector2(150.0f, toolbarRect.height);
            var previewButtonPosition = new Vector2(toolbarRect.xMax - previewButtonSize.x, toolbarRect.y);
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
                        new PreviewBreadcrumbOptions() { Heading = LocalizationManager.Instance.GetTranslation("result"), HighlightColor = Color.clear, Enabled = true, UseLeftStyle = true };
                    var breadcrumbSize = breadcrumbeOptions.SizeForContent;
                    breadcrumbSize.y = rect.height;
                    DrawPreviewBreadcrumb(new Rect(rect.position, breadcrumbSize), breadcrumbeOptions);
                }
            }
        }

        private void DrawOperationsPanel(Rect operationPanelRect)
        {
            var totalHeightOfOperations = 0.0f;
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                var drawer = RenameOperationsToApplyWithBindings[i].Drawer;
                totalHeightOfOperations += drawer.GetPreferredHeight();
            }

            var headerRect = new Rect(operationPanelRect);
            headerRect.height = 18.0f;
            this.DrawOperationsPanelHeader(headerRect);

            var scrollAreaRect = new Rect(operationPanelRect);
            scrollAreaRect.y += headerRect.height;
            scrollAreaRect.height -= headerRect.height;

            var buttonSize = new Vector2(150.0f, 20.0f);
            var spaceBetweenButton = 16.0f;
            var scrollContentsRect = new Rect(scrollAreaRect);
            scrollContentsRect.height = totalHeightOfOperations + spaceBetweenButton + buttonSize.y;

            // If we need to scroll vertically, subtract out room for the vertical scrollbar so we don't
            // have to also scroll horiztonally
            var contentsFit = scrollContentsRect.height <= scrollAreaRect.height;
            if (!contentsFit)
            {
                scrollContentsRect.width -= 15.0f;
            }

            this.renameOperationsPanelScrollPosition = GUI.BeginScrollView(
                scrollAreaRect,
                this.renameOperationsPanelScrollPosition,
                scrollContentsRect);

            this.DrawRenameOperations(scrollContentsRect);

            var buttonRect = new Rect();
            buttonRect.x = scrollContentsRect.x + (scrollContentsRect.size.x / 2.0f) - (buttonSize.x / 2.0f);
            buttonRect.y = scrollContentsRect.y + scrollContentsRect.height - buttonSize.y;
            buttonRect.height = buttonSize.y;
            buttonRect.width = buttonSize.x;

            if (GUI.Button(buttonRect, LocalizationManager.Instance.GetTranslation("addOperation")))
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

        private void DrawOperationsPanelHeader(Rect headerRect)
        {
            var headerStyle = new GUIStyle(EditorStyles.toolbar);
            GUI.Box(headerRect, "", headerStyle);
            var headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            var headerLabelRect = new Rect(headerRect);
            headerLabelRect.x += 2.0f;
            headerLabelRect.width -= 2.0f;

            var headerLabel = LocalizationManager.Instance.GetTranslation("renameOperation");
            var renameOpsLabel = new GUIContent(headerLabel);
            EditorGUI.LabelField(headerLabelRect, renameOpsLabel, headerLabelStyle);

            var preferencesButtonRect = new Rect(headerRect);
            preferencesButtonRect.width = 80.0f;
            preferencesButtonRect.x = headerRect.width - preferencesButtonRect.width;

            if (GUI.Button(preferencesButtonRect, LocalizationManager.Instance.GetTranslation("preferences"), EditorStyles.toolbarButton))
            {
                this.ShowPreferencesWindowForCurrentUnityVersion();
            }

            var presetButtonsRect = new Rect(headerRect);
            presetButtonsRect.width = 80.0f;
            presetButtonsRect.x = headerRect.width - presetButtonsRect.width - preferencesButtonRect.width;
            var useDebugPresets = Event.current.shift;
            if (GUI.Button(presetButtonsRect, LocalizationManager.Instance.GetTranslation("presets"), EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                var savedPresetNames = new string[this.ActivePreferences.SavedPresets.Count];
                for (int i = 0; i < this.ActivePreferences.SavedPresets.Count; ++i)
                {
                    savedPresetNames[i] = this.ActivePreferences.SavedPresets[i].Name;
                }

                for (int i = 0; i < savedPresetNames.Length; ++i)
                {
                    var content = new GUIContent(savedPresetNames[i]);
                    int copyI = i;
                    menu.AddItem(content, false, () =>
                    {
                        var preset = this.ActivePreferences.SavedPresets[copyI];
                        this.LoadPreset(preset);
                    });
                }

                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent(LocalizationManager.Instance.GetTranslation("saveAs")), false, () => this.ShowSavePresetWindow());
                menu.AddItem(new GUIContent(LocalizationManager.Instance.GetTranslation("managePresets")), false, () => this.ShowManagePresetsWindow());
                if (useDebugPresets)
                {
                    menu.AddItem(new GUIContent("DEBUG - Delete UserPrefs"), false, () =>
                    {
                        this.ActivePreferences.ResetToDefaults();
                        this.SaveUserPreferences();
                    });
                    menu.AddItem(new GUIContent("DEBUG - Reload Languages"), false, () =>
                    {
                        LocalizationManager.Instance.Initialize();
                    });
                }

                menu.ShowAsContext();
            }
        }

        private void ShowPreferencesWindowForCurrentUnityVersion()
        {
            // I used an API for preferences that was introduced in 2018.3 (SettingsProvider).
            // Draw a custom window for older versions
#if UNITY_2018_3_OR_NEWER
            SettingsService.OpenUserPreferences(MulliganSettingsProvider.Path);
#else
            MulliganUserPreferencesWindow.ShowWindow();
#endif
        }

        private void DrawRenameOperations(Rect operationsRect)
        {
            // Store the op before buttons are pressed because buttons change focus
            var focusedOpBeforeButtonPresses = this.FocusedRenameOp;
            bool saveOpsToPreferences = false;
            IRenameOperation operationToFocus = null;

            var totalHeightDrawn = 0.0f;
            for (int i = 0; i < this.NumRenameOperations; ++i)
            {
                var currentElement = this.RenameOperationsToApplyWithBindings[i];
                var rect = new Rect(operationsRect);
                rect.y += totalHeightDrawn;
                rect.height = currentElement.Drawer.GetPreferredHeight();
                totalHeightDrawn += rect.height;

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
                            var removingFocusedOperation = focusedOpBeforeButtonPresses == currentElement.Operation;

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
                            Debug.LogError(string.Format(LocalizationManager.Instance.GetTranslation("errorUnrecognizedListButton"), buttonClickEvent));
                            return;
                        }
                }

                if (operationToFocus != null)
                {
                    this.FocusRenameOperationDeferred(operationToFocus);
                }

                if (saveOpsToPreferences)
                {
                    this.SaveUserPreferences();
                }
            }
        }

        private void OnAddRenameOperationConfirmed(object operation)
        {
            var operationDrawerBinding = operation as RenameOperationDrawerBinding;
            if (operationDrawerBinding == null)
            {
                throw new System.ArgumentException(LocalizationManager.Instance.GetTranslation("errorAddNewOpNotSub"), operation.GetType().ToString());
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

            this.SaveUserPreferences();

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
            this.previewPanel.DisableAddSelectedObjectsButton = this.ValidSelectedObjects.Count == 0;
            this.previewPanel.InsertionTextColor = this.ActivePreferences.InsertionTextColor;
            this.previewPanel.InsertionBackgroundColor = this.ActivePreferences.InsertionBackgroundColor;
            this.previewPanel.DeletionTextColor = this.ActivePreferences.DeletionTextColor;
            this.previewPanel.DeletionBackgroundColor = this.ActivePreferences.DeletionBackgroundColor;
            this.previewPanelScrollPosition = this.previewPanel.Draw(previewPanelRect, this.previewPanelScrollPosition, bulkRenamePreview);
        }

        private void DrawReviewPrompt(Rect rect)
        {
            var reviewPrompt = string.Empty;
            Color color = Color.blue;
            if (ActivePreferences.HasConfirmedReviewPrompt)
            {
                color = new AddStringOperationDrawer().HighlightColor;
                if (RBPackageSettings.IsGitHubRelease)
                {
                    reviewPrompt = string.Format("<color=FFFFFFF>{0}</color>", LocalizationManager.Instance.GetTranslation("thankYouForSupport"));
                }
                else
                {
                    reviewPrompt = string.Format("<color=FFFFFFF>{0}</color>", LocalizationManager.Instance.GetTranslation("thankYouForReview"));
                }
            }
            else
            {
                color = new ReplaceNameOperationDrawer().HighlightColor;

                if (RBPackageSettings.IsGitHubRelease)
                {
                    reviewPrompt = string.Format("<color=FFFFFFF>{0}</color>", LocalizationManager.Instance.GetTranslation("thankYouForUsing"));
                }
                else
                {
                    reviewPrompt = string.Format("<color=FFFFFFF>{0}</color>", LocalizationManager.Instance.GetTranslation("thankYouForPurchasing"));
                }
            }

            DrawReviewBanner(rect, color, reviewPrompt, !ActivePreferences.HasConfirmedReviewPrompt);
        }

        private void DrawReviewBanner(Rect rect, Color color, string prompt, bool showButton)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            var reviewStyle = new GUIStyle(EditorStyles.largeLabel);
            reviewStyle.fontStyle = FontStyle.Bold;
            reviewStyle.alignment = TextAnchor.MiddleCenter;
            reviewStyle.wordWrap = true;
            reviewStyle.richText = true;

            var buttonRect = new Rect(rect);
            buttonRect.width = showButton ? 140.0f : 0.0f;
            buttonRect.height = 16.0f;
            var buttonPaddingLR = 10.0f;

            buttonRect.x = rect.width - (buttonRect.width + buttonPaddingLR);
            buttonRect.y += (rect.height * 0.5f) - (buttonRect.height * 0.5f);

            var labelRect = new Rect(rect);
            var labelPaddingL = 10.0f;
            labelRect.x += labelPaddingL;
            labelRect.width = (buttonRect.x - rect.x) - (buttonPaddingLR + labelPaddingL);

            GUI.Label(labelRect, prompt, reviewStyle);
            if (showButton && GUI.Button(buttonRect, LocalizationManager.Instance.GetTranslation("openAssetStore")))
            {
                this.ActivePreferences.HasConfirmedReviewPrompt = true;
                Application.OpenURL("https://assetstore.unity.com/packages/slug/99843");

                // Set a flag to continue to show the banner for this session
                this.IsShowingThanksForReview = true;
            }
        }

        private void ShowSavePresetWindow()
        {
            // Don't let them have both preset management windows open at once because it gets weird.
            if (this.activePresetManagementWindow != null)
            {
                this.activePresetManagementWindow.Close();
            }

            var existingWindow = this.activeSavePresetWindow;
            var windowMinSize = new Vector2(250.0f, 48.0f);
            var savePresetPosition = new Rect(this.position);
            savePresetPosition.size = windowMinSize;
            savePresetPosition.x = this.position.x + (this.position.width / 2.0f);
            savePresetPosition.y = this.position.y + (this.position.height / 2.0f);
            this.activeSavePresetWindow =
                EditorWindow.GetWindowWithRect<SavePresetWindow>(savePresetPosition, true, LocalizationManager.Instance.GetTranslation("savePreset"), true);
            this.activeSavePresetWindow.minSize = windowMinSize;
            this.activeSavePresetWindow.maxSize = new Vector2(windowMinSize.x * 2.0f, windowMinSize.y);
            this.activeSavePresetWindow.SetName(this.CurrentPresetName);
            this.activeSavePresetWindow.SetExistingPresetNames(this.ActivePreferences.PresetNames);

            // Only subscribe if it's a new, previously unopened window.
            if (existingWindow == null)
            {
                this.activeSavePresetWindow.PresetSaved += this.HandlePresetSaved;
            }
        }

        private void HandlePresetSaved(string presetName)
        {
            var savedPreset = this.SaveNewPresetFromCurrentOperations(presetName);
            this.LoadPreset(savedPreset);
        }

        private void ShowManagePresetsWindow()
        {
            // Don't let them have both preset management windows open at once because it gets weird.
            if (this.activeSavePresetWindow != null)
            {
                this.activeSavePresetWindow.Close();
            }

            var existingWindow = this.activePresetManagementWindow;
            this.activePresetManagementWindow = EditorWindow.GetWindow<ManagePresetsWindow>(true, LocalizationManager.Instance.GetTranslation("managePresets"), true);
            this.activePresetManagementWindow.PopulateWithPresets(this.ActivePreferences.SavedPresets);

            // Only subscribe if it's a new, previously unopened window.
            if (existingWindow == null)
            {
                this.activePresetManagementWindow.PresetsChanged += this.HandlePresetsChanged;
            }
        }

        private void HandlePresetsChanged(List<RenameSequencePreset> presets)
        {
            var presetCopies = new List<RenameSequencePreset>(presets.Count);
            foreach (var preset in presets)
            {
                var copySerialized = JsonUtility.ToJson(preset);
                var copy = JsonUtility.FromJson<RenameSequencePreset>(copySerialized);
                presetCopies.Add(copy);
            }

            this.ActivePreferences.SavedPresets = presetCopies;

            // Clear the current preset name if it no longer exists after they changed.
            // This way we don't write to a preset that doesn't exist (if we were to auto save changes back to the preset).
            // Also so we don't populate the Save As field with a name that's bogus.
            var currentPresetIndex = this.ActivePreferences.FindIndexOfSavedPresetWithName(this.CurrentPresetName);
            if (currentPresetIndex < 0)
            {
                this.CurrentPresetName = string.Empty;
            }
        }

        private void SaveUserPreferences()
        {
            var operationSequence = this.GetCurrentRenameOperationSequence();
            this.ActivePreferences.PreviousSequence = operationSequence;

            this.ActivePreferences.SaveToEditorPrefs();
        }

        private void LoadUserPreferences()
        {
            var oldSerializedOps = EditorPrefs.GetString(RenameOpsEditorPrefsKey, string.Empty);
            this.ActivePreferences = MulliganUserPreferences.LoadOrCreatePreferences();

            if (!string.IsNullOrEmpty(oldSerializedOps))
            {
                this.ActivePreferences.ResetColorsToDefault(EditorGUIUtility.isProSkin);

                // Update operations to the new preferences
                this.ActivePreferences.PreviousSequence = RenameOperationSequence<IRenameOperation>.FromString(oldSerializedOps);

                EditorPrefs.DeleteKey(RenameOpsEditorPrefsKey);
            }

            this.LoadOperationSequence(this.ActivePreferences.PreviousSequence);
            var originPreset = this.ActivePreferences.FindSavedPresetWithName(this.ActivePreferences.LastUsedPresetName);
            if (originPreset != null)
            {
                this.CurrentPresetName = originPreset.Name;
            }
        }

        private void LoadPreset(RenameSequencePreset preset)
        {
            this.CurrentPresetName = preset.Name;
            this.ActivePreferences.LastUsedPresetName = preset.Name;
            this.LoadOperationSequence(preset.OperationSequence);
        }

        private void LoadOperationSequence(RenameOperationSequence<IRenameOperation> sequence)
        {
            this.RenameOperationsToApplyWithBindings = new List<RenameOperationDrawerBinding>();

            foreach (var op in sequence)
            {
                // Find the drawer that goes with this operation's type
                foreach (var drawerBinding in this.RenameOperationDrawerBindingPrototypes)
                {
                    if (drawerBinding.Operation.GetType() == op.GetType())
                    {
                        this.AddRenameOperation(new RenameOperationDrawerBinding(op, drawerBinding.Drawer));
                        break;
                    }
                }
            }

            if (this.NumRenameOperations > 0)
            {
                this.FocusRenameOperationDeferred(this.RenameOperationsToApplyWithBindings.First().Operation);
            }
        }

        private RenameSequencePreset SaveNewPresetFromCurrentOperations(string presetName)
        {
            var preset = this.CreatePresetFromCurrentSequence(presetName);
            this.ActivePreferences.SavePreset(preset);

            return preset;
        }

        private RenameSequencePreset CreatePresetFromCurrentSequence(string presetName)
        {
            var operationSequence = this.GetCurrentRenameOperationSequence();
            var preset = new RenameSequencePreset()
            {
                Name = presetName,
                OperationSequence = operationSequence
            };

            return preset;
        }

        private RenameOperationSequence<IRenameOperation> GetCurrentRenameOperationSequence()
        {
            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            foreach (var binding in this.RenameOperationsToApplyWithBindings)
            {
                var clone = binding.Operation.Clone();
                operationSequence.Add(clone);
            }

            return operationSequence;
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
            this.AddObjectsToRename(this.ValidSelectedObjects);

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
        }
    }
}
