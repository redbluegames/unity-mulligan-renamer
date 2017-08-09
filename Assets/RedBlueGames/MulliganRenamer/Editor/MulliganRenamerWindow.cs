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
        private const string WindowMenuPath = "Window/Red Blue/Mulligan Renamer";

        private const string AddedTextColorTag = "<color=green>";
        private const string DeletedTextColorTag = "<color=red>";

        private const string RenameOpsEditorPrefsKey = "RedBlueGames.MulliganRenamer.RenameOperationsToApply";

        private const float PreviewPanelFirstColumnMinSize = 50.0f;

        private GUIStyles guiStyles;
        private GUIContents guiContents;
        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;
        private Rect scrollViewClippingRect;

        private BulkRenamer bulkRenamer;
        private List<RenameOperation> renameOperationPrototypes;
        private List<RenameOperation> renameOperationsToApply;
        private List<UnityEngine.Object> objectsToRename;

        private List<UnityEngine.Object> ObjectsToRename
        {
            get
            {
                if (this.objectsToRename == null)
                {
                    this.objectsToRename = new List<UnityEngine.Object>();
                }

                return this.objectsToRename;
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

            if (obj.GetType() == typeof(GameObject))
            {
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            AssetPreview.SetPreviewTextureCacheSize(100);
            this.minSize = new Vector2(600.0f, 300.0f);

            this.previewPanelScrollPosition = Vector2.zero;

            this.bulkRenamer = new BulkRenamer();
            this.renameOperationsToApply = new List<RenameOperation>();

            this.CacheRenameOperationPrototypes();

            this.LoadSavedRenameOperations();

            Selection.selectionChanged += this.Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= this.Repaint;
        }

        private void CacheRenameOperationPrototypes()
        {
            this.renameOperationPrototypes = new List<RenameOperation>();

            this.renameOperationPrototypes.Add(new ReplaceStringOperation());
            this.renameOperationPrototypes.Add(new ReplaceNameOperation());
            this.renameOperationPrototypes.Add(new AddStringOperation());
            this.renameOperationPrototypes.Add(new EnumerateOperation());
            this.renameOperationPrototypes.Add(new TrimCharactersOperation());
            this.renameOperationPrototypes.Add(new RemoveCharactersOperation());
            this.renameOperationPrototypes.Add(new ChangeCaseOperation());
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

            this.ObjectsToRename.Sort(delegate(UnityEngine.Object x, UnityEngine.Object y)
                {
                    return EditorUtility.NaturalCompare(x.name, y.name);
                });

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this.DrawOperationsPanel();
            this.DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30.0f);

            var disableRenameButton = this.RenameOperatationsHaveErrors() || this.ObjectsToRename.Count == 0;
            EditorGUI.BeginDisabledGroup(disableRenameButton);
            if (GUILayout.Button("Rename", GUILayout.Height(24.0f)))
            {
                this.bulkRenamer.RenameObjects(this.ObjectsToRename);
                this.ObjectsToRename.Clear();
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(30.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void InitializeGUIContents()
        {
            this.guiContents = new GUIContents();

            this.guiContents.DropPrompt = new GUIContent(
                "No objects specified for rename. Drag objects here to rename them, or");

            this.guiContents.DropPromptHint = new GUIContent(
                "Add more objects by dragging them here");
        }

        private void InitializeGUIStyles()
        {
            this.guiStyles = new GUIStyles();

            this.guiStyles.Icon = GUIStyle.none;
            this.guiStyles.OriginalNameLabelUnModified = EditorStyles.label;
            this.guiStyles.OriginalNameLabelWhenModified = EditorStyles.boldLabel;
            this.guiStyles.NewNameLabelUnModified = EditorStyles.label;
            this.guiStyles.NewNameLabelModified = EditorStyles.boldLabel;

            this.guiStyles.DropPrompt = new GUIStyle(EditorStyles.label);
            this.guiStyles.DropPrompt.alignment = TextAnchor.MiddleCenter;
            this.guiStyles.DropPromptHint = EditorStyles.centeredGreyMiniLabel;

            var previewHeaderStyle = new GUIStyle(EditorStyles.toolbar);
            var previewHeaderMargin = new RectOffset();
            previewHeaderMargin = previewHeaderStyle.margin;
            previewHeaderMargin.left = 1;
            previewHeaderMargin.right = 1;
            previewHeaderStyle.margin = previewHeaderMargin;
            this.guiStyles.PreviewHeader = previewHeaderStyle;

            if (EditorGUIUtility.isProSkin)
            {
                this.guiStyles.PreviewScroll = new GUIStyle(GUI.skin.FindStyle("CurveEditorBackground"));
            }
            else
            {
                this.guiStyles.PreviewScroll = new GUIStyle(EditorStyles.textArea);
            }
        }

        private void DrawOperationsPanel()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Rename Operations", EditorStyles.boldLabel);

            this.renameOperationsPanelScrollPosition = EditorGUILayout.BeginScrollView(
                this.renameOperationsPanelScrollPosition,
                GUILayout.MinWidth(200.0f),
                GUILayout.MaxWidth(350.0f));

            this.DrawRenameOperations();

            // BulkRenamer expects the list typed as IRenameOperations
            var renameOpsAsInterfaces = new List<IRenameOperation>();
            foreach (var renameOp in this.renameOperationsToApply)
            {
                renameOpsAsInterfaces.Add((IRenameOperation)renameOp);
            }

            this.bulkRenamer.SetRenameOperations(renameOpsAsInterfaces);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Operation", GUILayout.Width(150.0f)))
            {
                // Add enums to the menu
                var menu = new GenericMenu();
                for (int i = 0; i < this.renameOperationPrototypes.Count; ++i)
                {
                    var renameOp = this.renameOperationPrototypes[i];
                    var content = new GUIContent(renameOp.MenuDisplayPath);
                    menu.AddItem(content, false, this.OnAddRenameOperationConfirmed, renameOp);
                }

                menu.ShowAsContext();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRenameOperations()
        {
            for (int i = 0; i < this.renameOperationsToApply.Count; ++i)
            {
                var currentElement = this.renameOperationsToApply[i];
                var buttonClickEvent = currentElement.DrawGUI(i == 0, i == this.renameOperationsToApply.Count - 1);
                switch (buttonClickEvent)
                {
                    case RenameOperation.ListButtonEvent.MoveUp:
                        {
                            this.MoveRenameOpFromIndexToIndex(i, i - 1);
                            this.SaveRenameOperationsToPreferences();
                            break;
                        }

                    case RenameOperation.ListButtonEvent.MoveDown:
                        {
                            this.MoveRenameOpFromIndexToIndex(i, i + 1);
                            this.SaveRenameOperationsToPreferences();
                            break;
                        }

                    case RenameOperation.ListButtonEvent.Delete:
                        {
                            this.renameOperationsToApply.RemoveAt(i);
                            this.SaveRenameOperationsToPreferences();
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

                if (buttonClickEvent != RenameOperation.ListButtonEvent.None)
                {
                    // Workaround: Unfocus any focused control because otherwise it will select a field
                    // from the element that took this one's place.
                    GUI.FocusControl(string.Empty);

                    GUIUtility.ExitGUI();
                    return;
                }
            }
        }

        private void MoveRenameOpFromIndexToIndex(int fromIndex, int desiredIndex)
        {
            desiredIndex = Mathf.Clamp(desiredIndex, 0, this.renameOperationsToApply.Count - 1);
            var previousElement = this.renameOperationsToApply[desiredIndex];
            this.renameOperationsToApply[desiredIndex] = this.renameOperationsToApply[fromIndex];
            this.renameOperationsToApply[fromIndex] = previousElement;
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

            // Construct the Rename op
            var renameOp = operationAsRenameOp.Clone();
            this.renameOperationsToApply.Add(renameOp);

            this.SaveRenameOperationsToPreferences();

            // Scroll to the bottom to focus the newly created operation.
            this.ScrollRenameOperationsToBottom();
        }

        private void SaveRenameOperationsToPreferences()
        {
            var allOpPathsCommaSeparated = string.Empty;
            foreach (var op in this.renameOperationsToApply)
            {
                allOpPathsCommaSeparated += op.MenuDisplayPath;
                if (op != this.renameOperationsToApply.Last())
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
                this.renameOperationsToApply.Add(new ReplaceStringOperation());
            }
            else
            {
                var ops = serializedOps.Split(',');
                foreach (var op in ops)
                {
                    foreach (var prototypeOp in this.renameOperationPrototypes)
                    {
                        if (prototypeOp.MenuDisplayPath == op)
                        {
                            this.OnAddRenameOperationConfirmed(prototypeOp);
                            break;
                        }
                    }
                }
            }
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical();
            this.DrawPreviewTitle();

            this.previewPanelScrollPosition = EditorGUILayout.BeginScrollView(this.previewPanelScrollPosition, this.guiStyles.PreviewScroll);

            bool panelIsEmpty = this.ObjectsToRename.Count == 0;
            if (panelIsEmpty)
            {
                this.DrawPreviewPanelContentsEmpty();
            }
            else
            {
                this.DrawPreviewPanelContentsWithItems();
            }

            EditorGUILayout.EndScrollView();

            // GetLastRect only works during Repaint, so we cache it off during Repaint and use the cached value.
            if (Event.current.type == EventType.Repaint)
            {
                this.scrollViewClippingRect = GUILayoutUtility.GetLastRect();
            }

            var draggedObjects = this.GetDraggedObjectsOverRect(this.scrollViewClippingRect);
            if (draggedObjects.Count > 0)
            {
                this.ObjectsToRename.AddRange(draggedObjects);
                this.ScrollPreviewPanelToBottom();
            }

            if (!panelIsEmpty)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove All"))
                {
                    this.ObjectsToRename.Clear();
                }

                this.DrawAddSelectedObjectsButton();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewPanelContentsEmpty()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(this.guiContents.DropPrompt, this.guiStyles.DropPrompt);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            this.DrawAddSelectedObjectsButton();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

        private void DrawPreviewPanelContentsWithItems()
        {
            var previewContents = PreviewPanelContents.CreatePreviewContentsForObjects(this.bulkRenamer, this.ObjectsToRename);
            for (int i = 0; i < previewContents.NumRows; ++i)
            {
                if (this.DrawPreviewRow(previewContents[i], previewContents.LongestOriginalNameWidth, previewContents.LongestNewNameWidth))
                {
                    this.ObjectsToRename.Remove(this.ObjectsToRename[i]);
                    break;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(this.guiContents.DropPromptHint, this.guiStyles.DropPromptHint);
        }

        private void DrawAddSelectedObjectsButton()
        {
            var newlySelectedObjects = this.GetValidSelectedObjects();
            EditorGUI.BeginDisabledGroup(newlySelectedObjects.Count == 0);
            if (GUILayout.Button("Add Selected Objects"))
            {
                this.LoadSelectedObjects();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawPreviewTitle()
        {
            EditorGUILayout.BeginHorizontal(this.guiStyles.PreviewHeader);

            // Add space for the icons and remove buttons
            EditorGUILayout.LabelField("Rename Previews (Before / After)", EditorStyles.miniBoldLabel);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawPreviewRow(PreviewRowModel info, float firstColumnWidth, float secondColumnWidth)
        {
            bool isDeleteClicked = false;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(18.0f));

            // Space gives us a bit of padding or else we're just too bunched up to the side
            GUILayout.Space(4.0f);

            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16.0f)))
            {
                isDeleteClicked = true;
            }

            GUILayout.Box(info.Icon, this.guiStyles.Icon, GUILayout.Width(16.0f), GUILayout.Height(16.0f));

            var originalNameStyle = info.NamesAreDifferent ? this.guiStyles.OriginalNameLabelWhenModified : this.guiStyles.OriginalNameLabelUnModified;
            EditorGUILayout.LabelField(info.OriginalName, originalNameStyle, GUILayout.Width(firstColumnWidth));

            // Display new name
            var newNameStyle = info.NamesAreDifferent ? this.guiStyles.NewNameLabelModified : this.guiStyles.NewNameLabelUnModified;
            EditorGUILayout.LabelField(info.NewName, newNameStyle, GUILayout.Width(secondColumnWidth));
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            return isDeleteClicked;
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
            this.ObjectsToRename.AddRange(this.GetValidSelectedObjects());

            // Scroll to the bottom to focus the newly added objects.
            this.ScrollPreviewPanelToBottom();
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
            foreach (var renameOp in this.renameOperationsToApply)
            {
                if (renameOp.HasErrors)
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

        private struct PreviewRowModel
        {
            public Texture Icon { get; set; }

            public string OriginalName { get; set; }

            public string NewName { get; set; }

            public bool NamesAreDifferent
            {
                get
                {
                    return this.NewName != this.OriginalName;
                }
            }
        }

        private class PreviewPanelContents
        {
            private const float MinWidth = 200.0f;

            public float LongestOriginalNameWidth { get; private set; }

            public float LongestNewNameWidth { get; private set; }

            public int NumRows
            {
                get
                {
                    return this.PreviewRowInfos.Length;
                }
            }

            private PreviewRowModel[] PreviewRowInfos { get; set; }

            public PreviewRowModel this [int index]
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

            public static PreviewPanelContents CreatePreviewContentsForObjects(BulkRenamer renamer, List<UnityEngine.Object> objects)
            {
                var preview = new PreviewPanelContents();

                preview.PreviewRowInfos = new PreviewRowModel[objects.Count];
                var objectNames = objects.GetNames();
                var namePreviews = renamer.GetRenamePreviews(objectNames);

                for (int i = 0; i < namePreviews.Count; ++i)
                {
                    var info = new PreviewRowModel();
                    var namePreview = namePreviews[i];
                    info.OriginalName = namePreview.OriginalName;
                    info.NewName = namePreview.NewName;
                    info.Icon = GetIconForObject(objects[i]);

                    preview.PreviewRowInfos[i] = info;
                }

                float paddingForBold = 1.11f;
                preview.LongestOriginalNameWidth = 0.0f;
                preview.LongestNewNameWidth = 0.0f;
                foreach (var previewRowInfo in preview.PreviewRowInfos)
                {
                    float originalNameWidth = GUI.skin.label.CalcSize(new GUIContent(previewRowInfo.OriginalName)).x * paddingForBold;
                    if (originalNameWidth > preview.LongestOriginalNameWidth)
                    {
                        preview.LongestOriginalNameWidth = originalNameWidth;
                    }

                    float newNameWidth = GUI.skin.label.CalcSize(new GUIContent(previewRowInfo.NewName)).x * paddingForBold;
                    if (newNameWidth > preview.LongestNewNameWidth)
                    {
                        preview.LongestNewNameWidth = newNameWidth;
                    }
                }

                preview.LongestOriginalNameWidth = Mathf.Max(MinWidth, preview.LongestOriginalNameWidth);
                preview.LongestNewNameWidth = Mathf.Max(MinWidth, preview.LongestNewNameWidth);

                return preview;
            }

            private static Texture GetIconForObject(UnityEngine.Object unityObject)
            {
                var pathToObject = AssetDatabase.GetAssetPath(unityObject);
                Texture icon = null;
                if (string.IsNullOrEmpty(pathToObject))
                {
                    if (unityObject.GetType() == typeof(GameObject))
                    {
                        icon = EditorGUIUtility.FindTexture("GameObject Icon");
                    }
                    else
                    {
                        icon = EditorGUIUtility.FindTexture("DefaultAsset Icon");
                    }
                }
                else
                {
                    if (unityObject is Sprite)
                    {
                        icon = AssetPreview.GetAssetPreview(unityObject);
                    }
                    else
                    {
                        icon = AssetDatabase.GetCachedIcon(pathToObject);
                    }
                }

                return icon;
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

            public GUIStyle DropPrompt { get; set; }

            public GUIStyle DropPromptHint { get; set; }

            public GUIStyle PreviewHeader { get; set; }
        }

        private class GUIContents
        {
            public GUIContent DropPrompt { get; set; }

            public GUIContent DropPromptHint { get; set; }
        }
    }
}
