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
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Window to manage presets. Currently this is Renaming and Deleting, but it could be extended
    /// to support adding them to the right click menu.
    /// </summary>
    public class ManagePresetsWindow : EditorWindow
    {
        public event Action<List<RenameSequencePreset>> PresetsChanged;

        private List<RenameSequencePreset> presetsToDraw;
        private Dictionary<RenameSequencePreset, string> uniqueNames;

        private Vector2 scrollPosition;
        private ReorderableList reorderableList;

        public void PopulateWithPresets(List<RenameSequencePreset> presets)
        {
            this.uniqueNames = new Dictionary<RenameSequencePreset, string>();
            this.presetsToDraw = new List<RenameSequencePreset>(presets.Count);
            for (int i = 0; i < presets.Count; ++i)
            {
                var preset = presets[i];
                var copySerialized = JsonUtility.ToJson(preset);
                var copy = JsonUtility.FromJson<RenameSequencePreset>(copySerialized);
                this.presetsToDraw.Add(copy);
                this.uniqueNames.Add(copy, LocalizationManager.Instance.GetTranslation("preset") + " " + i);
            }

            this.reorderableList.list = this.presetsToDraw;
        }

        private void OnEnable()
        {
            this.reorderableList = new ReorderableList(
                new List<RenameSequencePreset>(),
                typeof(RenameSequencePreset),
                true,
                false,
                false,
                true);
            this.reorderableList.drawHeaderCallback = this.DrawHeader;
            this.reorderableList.drawElementCallback = this.DrawElement;
            this.reorderableList.onRemoveCallback = this.HandleElementRemoved;
            this.reorderableList.onReorderCallback = this.HandleReordered;
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, LocalizationManager.Instance.GetTranslation("savedPresets"), EditorStyles.label);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var preset = (RenameSequencePreset)this.reorderableList.list[index];

            var previousNameRect = new Rect(rect);
            previousNameRect.width = rect.width * 0.25f;
            previousNameRect.height = EditorGUIUtility.singleLineHeight;
            previousNameRect.y += (rect.height - previousNameRect.height) / 2.0f;
            EditorGUI.LabelField(previousNameRect, this.uniqueNames[preset]);

            var newNameRect = new Rect(rect);
            newNameRect.width = rect.width - previousNameRect.width;
            newNameRect.x = previousNameRect.xMax;
            newNameRect.height = previousNameRect.height;
            newNameRect.y += (rect.height - newNameRect.height) / 2.0f;
            var newName = EditorGUI.TextField(newNameRect, preset.Name);

            // Don't let them name is an empty name
            if (string.IsNullOrEmpty(newName))
            {
                newName = preset.Name;
            }

            // Don't let them name two presets the same thing.
            for (int i = 0; i < this.presetsToDraw.Count; ++i)
            {
                var existingPreset = this.presetsToDraw[i];
                if (i != index && newName == existingPreset.Name)
                {
                    newName = preset.Name;
                }
            }

            if (newName != preset.Name)
            {
                this.presetsToDraw[index].Name = newName;
                this.InvokePresetsChanged();
            }

            return;
        }

        private void OnGUI()
        {
            if (this.reorderableList.count > 0)
            {
                var padding = 10;
                var windowRect = this.position;
                windowRect.x = 0;
                windowRect.y = 0;
                var contentsHeight =
                    (this.reorderableList.elementHeight * this.reorderableList.count) +
                    this.reorderableList.headerHeight +
                    this.reorderableList.footerHeight +
                    padding * 2;
                var contentsRect = new Rect(windowRect);
                contentsRect.height = contentsHeight;
                this.scrollPosition = GUI.BeginScrollView(windowRect, this.scrollPosition, contentsRect);
                var paddedRect = windowRect.AddPadding(padding, padding, padding, padding);
                this.reorderableList.DoList(paddedRect);
                GUI.EndScrollView();
            }
            else
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50.0f);
                EditorGUILayout.LabelField(
                    LocalizationManager.Instance.GetTranslation("errorNoSavedPresets"),
                    EditorStyles.wordWrappedLabel);
                GUILayout.Space(50.0f);
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
        }

        private void HandleElementRemoved(UnityEditorInternal.ReorderableList list)
        {
            var indexToRemove = list.index;
            var elementToDelete = this.presetsToDraw[indexToRemove];
            var popupMessage = string.Format(
                LocalizationManager.Instance.GetTranslation("areYouSureDelete"), elementToDelete.Name
            );

            if (EditorUtility.DisplayDialog(LocalizationManager.Instance.GetTranslation("warning"),
                                            popupMessage,
                                            LocalizationManager.Instance.GetTranslation("deletePreset"),
                                            LocalizationManager.Instance.GetTranslation("no")))
            {
                this.presetsToDraw.RemoveAt(indexToRemove);
                this.reorderableList.index = 0;
                this.InvokePresetsChanged();
            }
        }

        private void HandleReordered(UnityEditorInternal.ReorderableList list)
        {
            this.InvokePresetsChanged();
        }

        private void InvokePresetsChanged()
        {
            if (this.PresetsChanged != null)
            {
                this.PresetsChanged.Invoke(this.presetsToDraw);
            }
        }
    }
}