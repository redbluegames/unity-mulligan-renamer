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

namespace RedBlueGames.Tools
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Tool that tries to allow renaming mulitple selections by parsing similar substrings
    /// </summary>
    public class BulkRenamerWindow : EditorWindow
    {
        private const string AssetsMenuPath = "Assets/Red Blue/Rename In Bulk";
        private const string GameObjectMenuPath = "GameObject/Red Blue/Rename In Bulk";

        private Vector2 previewPanelScrollPosition;
        private List<UnityEngine.Object> objectsToRename;
        private BulkRenamer bulkRenamer;

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

        private static void DrawPreviewRow(Texture icon, string originalName, string diffedName, string newName)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(18.0f));
            GUILayout.Space(8.0f);
            if (icon != null)
            {
                GUIStyle boxStyle = GUIStyle.none;
                GUILayout.Box(icon, boxStyle, GUILayout.Width(16.0f), GUILayout.Height(16.0f));
            }

            // Calculate if names differ for use with styles
            bool namesDiffer = newName != originalName;

            // Display diff
            var diffStyle = namesDiffer ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
            diffStyle.richText = true;
            EditorGUILayout.LabelField(diffedName, diffStyle);

            // Display new name
            var style = namesDiffer ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
            EditorGUILayout.LabelField(newName, style);

            EditorGUILayout.EndHorizontal();
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
            this.previewPanelScrollPosition = Vector2.zero;
            this.bulkRenamer = new BulkRenamer();

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
                this.objectsToRename = new List<Object>();
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

            EditorGUILayout.LabelField("Text Replacement", EditorStyles.boldLabel);
            this.bulkRenamer.SearchString = EditorGUILayout.TextField(
                "Search for String",
                this.bulkRenamer.SearchString);
            this.bulkRenamer.ReplacementString = EditorGUILayout.TextField(
                "Replace with",
                this.bulkRenamer.ReplacementString);

            this.bulkRenamer.SearchIsCaseSensitive = EditorGUILayout.Toggle(
                "Case Sensitive",
                this.bulkRenamer.SearchIsCaseSensitive);

            EditorGUILayout.LabelField("Additions", EditorStyles.boldLabel);
            this.bulkRenamer.Prefix = EditorGUILayout.TextField("Prefix", this.bulkRenamer.Prefix);
            this.bulkRenamer.Suffix = EditorGUILayout.TextField("Suffix", this.bulkRenamer.Suffix);

            EditorGUILayout.LabelField("Trimming", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            this.bulkRenamer.NumFrontDeleteChars = EditorGUILayout.IntField(
                "Delete From Front",
                this.bulkRenamer.NumFrontDeleteChars);
            this.bulkRenamer.NumFrontDeleteChars = Mathf.Max(0, this.bulkRenamer.NumFrontDeleteChars);
            this.bulkRenamer.NumBackDeleteChars = EditorGUILayout.IntField(
                "Delete from Back",
                this.bulkRenamer.NumBackDeleteChars);
            this.bulkRenamer.NumBackDeleteChars = Mathf.Max(0, this.bulkRenamer.NumBackDeleteChars);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Enumerating", EditorStyles.boldLabel);
            this.bulkRenamer.CountFormat = EditorGUILayout.TextField(
                "Count Format",
                this.bulkRenamer.CountFormat);

            try
            {
                this.bulkRenamer.StartingCount.ToString(this.bulkRenamer.CountFormat);
            }
            catch (System.FormatException)
            {
                var helpBoxMessage = "Invalid Count Format. Typical formats are D1 for one digit with no " +
                                     "leading zeros, D2, for two, etc." +
                                     "\nSee https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx" +
                                     " for more formatting options.";
                EditorGUILayout.HelpBox(helpBoxMessage, MessageType.Warning);
            }

            this.bulkRenamer.StartingCount = EditorGUILayout.IntField(
                "Count From",
                this.bulkRenamer.StartingCount);

            if (GUILayout.Button("Rename"))
            {
                this.RenameAssets();
                this.Close();
            }

            EditorGUILayout.Space();
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));

            DrawPreviewTitle();

            this.previewPanelScrollPosition = EditorGUILayout.BeginScrollView(this.previewPanelScrollPosition);
            var selectedNames = this.GetNamesFromObjectsToRename();
            var namePreviews = this.bulkRenamer.GetRenamedStrings(false, selectedNames);
            var nameDiffs = this.bulkRenamer.GetRenamedStrings(true, selectedNames);
            for (int i = 0; i < namePreviews.Length; ++i)
            {
                DrawPreviewRow(GetIconForObject(this.objectsToRename[i]), selectedNames[i], nameDiffs[i], namePreviews[i]);
            }

            EditorGUILayout.EndScrollView();
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
    }
}