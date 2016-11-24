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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Tool that tries to allow renaming mulitple selections by parsing similar substrings
    /// </summary>
    public class BulkRename : EditorWindow
    {
        private const string MenuPath = "Assets/Rename In Bulk";
        private const string AddedTextColorTag = "<color=green>";
        private const string DeletedTextColorTag = "<color=red>";
        private const string EndColorTag = "</color>";

        private List<UnityEngine.Object> objectsToRename;

        private string prefix;
        private string suffix;
        private string searchToken;
        private string replacementString;

        private string RichTextPrefix
        {
            get
            {
                return string.Concat(AddedTextColorTag, this.prefix, EndColorTag);
            }
        }

        private string RichTextSuffix
        {
            get
            {
                return string.Concat(AddedTextColorTag, this.suffix, EndColorTag);
            }
        }

        private string RichTextReplacementString
        {
            get
            {
                return string.Concat(
                    DeletedTextColorTag,
                    this.searchToken,
                    EndColorTag,
                    AddedTextColorTag,
                    this.replacementString,
                    EndColorTag);
            }
        }

        [MenuItem(MenuPath)]
        private static void ShowRenameSpritesheetWindow()
        {
            EditorWindow.GetWindow<BulkRename>(true, "Bulk Rename", true);
        }

        [MenuItem(MenuPath, true)]
        private static bool IsSelectionValid()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }

            return true;
        }

        private static bool ObjectIsValidForRename(UnityEngine.Object obj)
        {
            return AssetDatabase.Contains(obj);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += this.Repaint;

            this.prefix = string.Empty;
            this.suffix = string.Empty;
            this.searchToken = string.Empty;
            this.replacementString = string.Empty;

            this.RefreshObjectsToRename();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= this.Repaint;
        }

        private void RefreshObjectsToRename()
        {
            var selectedObjects = Selection.objects;
            this.objectsToRename = new List<UnityEngine.Object>();
            foreach (var selectedObject in selectedObjects)
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
                "The Bulk Rename tool tries to allow renaming mulitple selections by parsing similar substrings",
                MessageType.None);

            EditorGUILayout.Space();

            if (this.objectsToRename.Count == 0)
            {
                EditorGUILayout.HelpBox("No objects selected. Select some objects to rename.", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Additions", EditorStyles.boldLabel);
            this.prefix = EditorGUILayout.TextField("Prefix", this.prefix);
            this.suffix = EditorGUILayout.TextField("Suffix", this.suffix);

            EditorGUILayout.LabelField("Text Replacement", EditorStyles.boldLabel);

            this.searchToken = EditorGUILayout.TextField("Search Token", this.searchToken);
            this.replacementString = EditorGUILayout.TextField("Replace with", this.replacementString);

            if (GUILayout.Button("Rename"))
            {
                this.RenameAssets();
                this.Close();
            }

            EditorGUILayout.Space();
            GUILayout.Box(string.Empty, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Diff", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("New Name", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            foreach (var objectToRename in this.objectsToRename)
            {
                EditorGUILayout.BeginHorizontal();

                // Display diff
                var previewObjectname = this.GetNewNameForAsset(objectToRename, false);
                var objectName = objectToRename.name;
                bool namesDiffer = previewObjectname != objectName;
                var diffStyle = namesDiffer ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
                diffStyle.richText = true;
                EditorGUILayout.LabelField(this.GetNewNameForAsset(objectToRename, true), diffStyle);

                // Display new name
                var style = namesDiffer ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label);
                EditorGUILayout.LabelField(previewObjectname, style);

                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenameAssets()
        {
            for (int i = 0; i < this.objectsToRename.Count; ++i)
            {
                var asset = this.objectsToRename[i];

                var infoString = string.Format(
                                     "Renaming asset {0} of {1}",
                                     i,
                                     this.objectsToRename.Count);

                EditorUtility.DisplayProgressBar(
                    "Renaming Assets...",
                    infoString,
                    i / (float)this.objectsToRename.Count);
                
                this.RenameAsset(asset);
            }

            EditorUtility.ClearProgressBar();
        }

        private void RenameAsset(UnityEngine.Object asset)
        {
            var pathToAsset = AssetDatabase.GetAssetPath(asset);
            var newName = this.GetNewNameForAsset(asset, false);
            AssetDatabase.RenameAsset(pathToAsset, newName);
        }

        private string GetNewNameForAsset(UnityEngine.Object asset, bool useRichText)
        {
            var modifiedName = asset.name;

            // Replace strings first so we don't replace the prefix.
            if (!string.IsNullOrEmpty(this.searchToken))
            {
                var replacementString = useRichText ? this.RichTextReplacementString :
                    this.replacementString;
                modifiedName = asset.name.Replace(this.searchToken, replacementString);
            }

            if (!string.IsNullOrEmpty(this.prefix))
            {
                var prefix = useRichText ? this.RichTextPrefix : this.prefix;
                modifiedName = string.Concat(prefix, modifiedName);
            }

            if (!string.IsNullOrEmpty(this.suffix))
            {
                var suffix = useRichText ? this.RichTextSuffix : this.suffix;
                modifiedName = string.Concat(modifiedName, suffix);
            }

            return modifiedName;
        }
    }
}