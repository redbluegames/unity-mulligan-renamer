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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Tool that tries to allow renaming mulitple selections by parsing similar substrings
    /// </summary>
    public class BulkRenamerWindow : EditorWindow
    {
        private const string AssetsMenuPath = "Assets/Red Blue/Rename In Bulk";
        private const string GameObjectMenuPath = "GameObject/Red Blue/Rename In Bulk";

        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;
        private List<UnityEngine.Object> objectsToRename;

        private List<BaseRenameOperation> renameOperationsFactory;

        private BulkRenamer bulkRenamer;
        private List<BaseRenameOperation> renameOperationsToApply;

        [MenuItem(AssetsMenuPath, false, 1011)]
        [MenuItem(GameObjectMenuPath, false, 49)]
        private static void ShowRenameSpritesheetWindow()
        {
            EditorWindow.GetWindow<BulkRenamerWindow>(true, "Bulk Rename", true);
        }

        [MenuItem(AssetsMenuPath, true)]
        [MenuItem(GameObjectMenuPath, true)]
        private static bool IsAssetSelectionValid()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }

            // Allow a rename if any valid object is selected. The invalid ones won't be renamed.
            foreach (var selection in Selection.objects)
            {
                if (ObjectIsValidForRename(selection))
                {
                    return true;
                }
            }

            return false;
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

        private static void DrawPreviewTitle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(32.0f);
            EditorGUILayout.LabelField("Diff", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("New Name", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawPreviewRow(PreviewRowInfo info)
        {
            // Draw the icon
            EditorGUILayout.BeginHorizontal(GUILayout.Height(18.0f));
            GUILayout.Space(8.0f);
            if (info.Icon != null)
            {
                GUIStyle boxStyle = GUIStyle.none;
                GUILayout.Box(info.Icon, boxStyle, GUILayout.Width(16.0f), GUILayout.Height(16.0f));
            }

            // Display diff
            var diffStyle = info.NamesAreDifferent ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
            diffStyle.richText = true;
            EditorGUILayout.LabelField(info.DiffName, diffStyle);

            // Display new name
            var style = info.NamesAreDifferent ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
            EditorGUILayout.LabelField(info.NewName, style);

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawDivider()
        {
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(2.0f));
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
                icon = AssetDatabase.GetCachedIcon(pathToObject);
            }

            return icon;
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(600.0f, 300.0f);

            this.previewPanelScrollPosition = Vector2.zero;

            this.bulkRenamer = new BulkRenamer();
            this.renameOperationsToApply = new List<BaseRenameOperation>();
            this.renameOperationsToApply.Add(new ReplaceStringOperation());

            // Cache all valid Rename Operations
            this.renameOperationsFactory = new List<BaseRenameOperation>();
            var assembly = Assembly.Load(new AssemblyName("Assembly-CSharp-Editor"));
            var typesInAssembly = assembly.GetTypes();
            foreach (var type in typesInAssembly)
            {
                if (type.IsSubclassOf(typeof(BaseRenameOperation)))
                {
                    var renameOp = (BaseRenameOperation)System.Activator.CreateInstance(type);
                    this.renameOperationsFactory.Add(renameOp);
                }
            }

            this.renameOperationsFactory.Sort((x, y) =>
                {
                    return x.MenuOrder.CompareTo(y.MenuOrder);
                });

            Selection.selectionChanged += this.Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= this.Repaint;
        }

        private void RefreshObjectsToRename()
        {
            if (this.objectsToRename == null)
            {
                this.objectsToRename = new List<UnityEngine.Object>();
            }

            this.objectsToRename.Clear();

            foreach (var selectedObject in RBSelection.SelectedObjectsSortedByTime)
            {
                if (ObjectIsValidForRename(selectedObject))
                {
                    this.objectsToRename.Add(selectedObject);
                }
            }
        }

        private void OnGUI()
        {
            this.RefreshObjectsToRename();

            EditorGUILayout.HelpBox(
                "BulkRename allows renaming mulitple selections at one time via string replacement and other methods.",
                MessageType.None);

            if (this.objectsToRename.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No objects selected. Select some Assets or scene Objects to rename.",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            this.renameOperationsPanelScrollPosition = 
                EditorGUILayout.BeginScrollView(
                this.renameOperationsPanelScrollPosition,
                GUILayout.MinWidth(200.0f),
                GUILayout.MaxWidth(350.0f));

            for (int i = 0; i < this.renameOperationsToApply.Count; ++i)
            {
                var currentElement = this.renameOperationsToApply[i];
                var clickEvent = currentElement.DrawGUI(i == 0, i == this.renameOperationsToApply.Count - 1);
                switch (clickEvent)
                {
                    case BaseRenameOperation.ListButtonEvent.MoveUp:
                        {
                            var indexOfAboveElement = Mathf.Max(0, i - 1);
                            var previousElement = this.renameOperationsToApply[indexOfAboveElement];
                            this.renameOperationsToApply[indexOfAboveElement] = currentElement;
                            this.renameOperationsToApply[i] = previousElement;

                            // Workaround: Unfocus any focused control because otherwise it will select a field
                            // from the element that took this one's place.
                            GUI.FocusControl(string.Empty);

                            GUIUtility.ExitGUI();
                            return;
                        }

                    case BaseRenameOperation.ListButtonEvent.MoveDown:
                        {
                            var indexOfBelowElement = Mathf.Min(this.renameOperationsToApply.Count - 1, i + 1);
                            var nextElement = this.renameOperationsToApply[indexOfBelowElement];
                            this.renameOperationsToApply[indexOfBelowElement] = currentElement;
                            this.renameOperationsToApply[i] = nextElement;

                            // Workaround: Unfocus any focused control because otherwise it will select a field
                            // from the element that took this one's place.
                            GUI.FocusControl(string.Empty);

                            GUIUtility.ExitGUI();
                            return;
                        }

                    case BaseRenameOperation.ListButtonEvent.Delete:
                        {
                            this.renameOperationsToApply.RemoveAt(i);

                            // Workaround: Unfocus any focused control because otherwise it will select a field
                            // from the element that took this one's place.
                            GUI.FocusControl(string.Empty);

                            GUIUtility.ExitGUI();
                            return;
                        }

                    case BaseRenameOperation.ListButtonEvent.None:
                        {
                            // Do nothing
                            break;
                        }

                    default:
                        {
                            Debug.LogError(string.Format(
                                    "RenamerWindow found Unrecognized ListButtonEvent [{0}] in OnGUI. Add a case to handle this event.", 
                                    clickEvent));
                            return;
                        }
                }

                DrawDivider();
            }

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
                for (int i = 0; i < this.renameOperationsFactory.Count; ++i)
                {
                    var renameOp = this.renameOperationsFactory[i];
                    var content = new GUIContent(renameOp.MenuDisplayPath);
                    menu.AddItem(content, false, this.OnAddRenameOperationConfirmed, renameOp);
                }

                menu.ShowAsContext();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            GUILayout.Box(string.Empty, GUILayout.ExpandHeight(true), GUILayout.Width(3.0f));

            EditorGUILayout.BeginVertical();

            this.previewPanelScrollPosition = EditorGUILayout.BeginScrollView(this.previewPanelScrollPosition);

            // Note that something about the way we draw the preview title, requires it to be included in the scroll view in order
            // for the scroll to measure horiztonal size correctly.
            DrawPreviewTitle();

            var previewRowData = this.GetPreviewRowDataFromObjectsToRename();
            for (int i = 0; i < previewRowData.Length; ++i)
            {
                DrawPreviewRow(previewRowData[i]);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30.0f);

            EditorGUI.BeginDisabledGroup(this.RenameOperatationsHaveErrors());
            if (GUILayout.Button("Rename", GUILayout.Height(24.0f)))
            {
                this.RenameAssets();
                this.Close();
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(30.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void OnAddRenameOperationConfirmed(object operation)
        {
            var operationAsRenameOp = operation as BaseRenameOperation;
            if (operationAsRenameOp == null)
            {
                throw new System.ArgumentException(
                    "BulkRenamerWindow tried to add a new RenameOperation using a type that is not a subclass of BaseRenameOperation." +
                    " Operation type: " +
                    operation.GetType().ToString());
            }

            // Construct the Rename op
            var renameOp = operationAsRenameOp.Clone();
            this.renameOperationsToApply.Add(renameOp);

            // Scroll to the bottom to focus the newly created operation.
            this.renameOperationsPanelScrollPosition = new Vector2(0.0f, 10000000.0f);
            this.Repaint();
        }

        private PreviewRowInfo[] GetPreviewRowDataFromObjectsToRename()
        {
            var previewRowInfos = new PreviewRowInfo[this.objectsToRename.Count];
            var selectedNames = this.GetNamesFromObjectsToRename();
            var namePreviews = this.bulkRenamer.GetRenamedStrings(false, selectedNames);
            var nameDiffs = this.bulkRenamer.GetRenamedStrings(true, selectedNames);

            for (int i = 0; i < selectedNames.Length; ++i)
            {
                var info = new PreviewRowInfo();
                info.OriginalName = selectedNames[i];
                info.DiffName = nameDiffs[i];
                info.NewName = namePreviews[i];
                info.Icon = GetIconForObject(this.objectsToRename[i]);

                previewRowInfos[i] = info;
            }

            return previewRowInfos;
        }

        private string[] GetNamesFromObjectsToRename()
        {
            int namesCount = this.objectsToRename.Count;
            var names = new string[namesCount];
            for (int i = 0; i < namesCount; ++i)
            {
                names[i] = this.objectsToRename[i].name;
            }

            return names;
        }

        private void RenameAssets()
        {
            // Record all the objects to undo stack, though this unfortunately doesn't capture Asset renames
            Undo.RecordObjects(this.objectsToRename.ToArray(), "Bulk Rename");

            var names = this.GetNamesFromObjectsToRename();
            var newNames = this.bulkRenamer.GetRenamedStrings(false, names);

            for (int i = 0; i < newNames.Length; ++i)
            {
                var infoString = string.Format(
                                     "Renaming asset {0} of {1}",
                                     i,
                                     newNames.Length);

                EditorUtility.DisplayProgressBar(
                    "Renaming Assets...",
                    infoString,
                    i / (float)newNames.Length);

                this.RenameObject(this.objectsToRename[i], newNames[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private void RenameObject(UnityEngine.Object obj, string newName)
        {
            if (AssetDatabase.Contains(obj))
            {
                this.RenameAsset(obj, newName);
            }
            else
            {
                this.RenameGameObject(obj, newName);
            }
        }

        private void RenameGameObject(UnityEngine.Object gameObject, string newName)
        {
            gameObject.name = newName;
        }

        private void RenameAsset(UnityEngine.Object asset, string newName)
        {
            var pathToAsset = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.RenameAsset(pathToAsset, newName);
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

        private struct PreviewRowInfo
        {
            public Texture Icon { get; set; }

            public string OriginalName { get; set; }

            public string DiffName { get; set; }

            public string NewName { get; set; }

            public bool NamesAreDifferent
            {
                get
                {
                    return this.NewName != this.OriginalName;
                }
            }
        }
    }
}
