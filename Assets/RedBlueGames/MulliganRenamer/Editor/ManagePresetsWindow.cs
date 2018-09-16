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

    public class ManagePresetsWindow : EditorWindow
    {
        private List<RenameSequencePreset> presetsToDraw;

        public event Action<int> PresetDeleted;

        public event Action<int, string> PresetRenamed;

        public void PopulateWithPresets(List<RenameSequencePreset> presets)
        {
            this.presetsToDraw = new List<RenameSequencePreset>(presets.Count);
            foreach (var preset in presets)
            {
                var copySerialized = JsonUtility.ToJson(preset);
                var copy = JsonUtility.FromJson<RenameSequencePreset>(copySerialized);
                this.presetsToDraw.Add(copy);
            }
        }

        private void OnEnable()
        {
            this.PresetDeleted += this.HandlePresetDeleted;
            this.PresetRenamed += this.HandlePresetRenamed;
        }

        private void OnGUI()
        {
            for (int i = 0; i < this.presetsToDraw.Count; ++i)
            {
                if (DrawPreset(i, presetsToDraw[i]))
                {
                    GUIUtility.ExitGUI();
                    break;
                }
            }

            GUILayout.Button("Restore Defaults");
        }

        private bool DrawPreset(int index, RenameSequencePreset preset)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(18.0f)))
            {
                if (this.PresetDeleted != null)
                {
                    this.PresetDeleted(index);
                    return true;
                }
            }

            var newName = EditorGUILayout.TextField(preset.Name);
            if (newName != preset.Name)
            {
                if (this.PresetRenamed != null)
                {
                    this.PresetRenamed(index, newName);
                }
            }

            EditorGUILayout.EndHorizontal();

            return false;
        }

        private void HandlePresetDeleted(int index)
        {
            this.presetsToDraw.RemoveAt(index);
        }

        private void HandlePresetRenamed(int index, string newName)
        {
            this.presetsToDraw[index].Name = newName;
        }
    }
}