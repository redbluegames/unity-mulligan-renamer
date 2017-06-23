namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The utility 'View', using Unity's Editor GUI framework.
    /// </summary>
    internal class StyleCopIgnoreUtilityWindow : EditorWindow, IStyleCopIgnoreUtilityView
    {
        /* Consts, Fields ====================================================================================================== */

        private StyleCopIgnoreUtility styleCopIgnoreUtility;
        private FileTreeView fileTreeInfo;
        private Vector2 treeViewScrollPosition;
        private int indentLevel;

        /* Methods ============================================================================================================ */

        /// <summary>
        /// Sets a reference to the Utility in order to communicate with it
        /// </summary>
        /// <param name="utility">The StyleCopIgnoreUtility</param>
        public void SetUtility(StyleCopIgnoreUtility utility)
        {
            this.styleCopIgnoreUtility = utility;
        }

        /// <summary>
        /// Reads and applies user preferences to customize the View
        /// </summary>
        /// <param name="preferences">User preferences to read from</param>
        public void ReadPreferences(Preferences preferences)
        {
            this.position = preferences.WindowPosition;
        }

        /// <summary>
        /// Saves user preferences to customize the View
        /// </summary>
        /// <param name="preferences">User preferences to save to</param>
        public void WritePreferences(Preferences preferences)
        {
            this.styleCopIgnoreUtility.UserPreferences.WindowPosition = this.position;
        }

        /// <summary>
        /// Gets all of the files that are currently selected ('checked') in the View
        /// </summary>
        /// <returns>Returns a List of strings containing complete, full file paths (including the filename and extension)</returns>
        public List<UniversalPath> GetSelectedFiles()
        {
            return this.fileTreeInfo.GetSelectedFiles();
        }

        /// <summary>
        /// Initialize the View with the provided saved data so that the View can reflect the data's current state
        /// </summary>
        /// <returns>Returns true if the operation suceeded, otherwise false</returns>
        /// <param name="saveData">Save data.</param>
        public bool InitializeWithData(StyleCopIgnoreUtilityData saveData)
        {
            var filesToSetIgnored = new List<UniversalPath>(saveData.IgnoredFilePaths);
            var result = this.fileTreeInfo.SetSelectedFiles(ref filesToSetIgnored);

            if (filesToSetIgnored.Count > 0)
            {
                const int MaxRemovedFilesToShow = 10;

                var removedFilePaths = string.Empty;
                for (int i = 0; i < filesToSetIgnored.Count; ++i)
                {
                    removedFilePaths += (string)filesToSetIgnored[i] + Environment.NewLine;

                    if (i >= (MaxRemovedFilesToShow - 1))
                    {
                        removedFilePaths += "..." + Environment.NewLine + "+" + (filesToSetIgnored.Count - MaxRemovedFilesToShow).ToString() + " more";
                        break;
                    }
                }

                var errorMessage = "The saved list of files contains names of files that no longer seem to exist.  Would you " +
                                   "like to remove these nonexistent saved file names now?" + Environment.NewLine + Environment.NewLine +
                                   removedFilePaths;

                if (EditorUtility.DisplayDialog(StyleCopIgnoreUtility.UtilityName, errorMessage, "Yes", "No"))
                {
                    this.styleCopIgnoreUtility.OnSave();
                }
            }

            return result;
        }

        /// <summary>
        /// The entry-point to the utility, called when the user selects the tool from the Menu.
        /// </summary>
        [MenuItem("Window/StyleCop Ignore Utility")]
        private static void CreateWindow()
        {
            var window = (StyleCopIgnoreUtilityWindow)GetWindow(typeof(StyleCopIgnoreUtilityWindow));
            window.Show();

            StyleCopIgnoreUtility.CreateWithView(window);
        }

        private void OnEnable()
        {
            this.titleContent = new GUIContent(StyleCopIgnoreUtility.WindowName);
            this.minSize = new Vector2(275, 300);

            this.fileTreeInfo = new FileTreeView((UniversalPath)Application.dataPath);
        }

        private void OnDestroy()
        {
            if (this.styleCopIgnoreUtility != null)
            {
                this.styleCopIgnoreUtility.OnQuit();
            }
        }

        private void OnInspectorUpdate()
        {
            if ((this.styleCopIgnoreUtility == null) || (this.styleCopIgnoreUtility.SaveData == null))
            {
                this.Close();
                return;
            }

            this.Repaint();
        }

        private void OnGUI()
        {
            if ((this.styleCopIgnoreUtility == null) || (this.styleCopIgnoreUtility.SaveData == null))
            {
                GUIUtility.ExitGUI();
                return;
            }

            this.indentLevel = 0;

            EditorGUILayout.Space();
            this.OnGUIFilePath();

            EditorGUILayout.Space();
            this.OnGUIFileTree();

            this.OnGUIStatusBar();

            EditorGUILayout.Space();
            this.OnGUIFooter();

            EditorGUILayout.Space();
        }

        private void OnGUIFilePath()
        {
            var filePathText = this.styleCopIgnoreUtility.SaveData.StyleCopSettingsFilePath.IsValid ?
                (string)this.styleCopIgnoreUtility.SaveData.StyleCopSettingsFilePath :
                "<not set>";

            EditorGUILayout.LabelField("StyleCop Settings File:");

            EditorGUILayout.BeginHorizontal();
            this.indentLevel++;
            this.ManualGUIIndent();
            EditorGUILayout.LabelField(filePathText);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var newFilePath = EditorUtility.OpenFilePanel(
                                      "StyleCop Settings File...",
                                      string.Empty,
                                      StyleCopIgnoreUtility.StyleCopFileExtension);

                if (!string.IsNullOrEmpty(newFilePath))
                {
                    try
                    {
                        this.styleCopIgnoreUtility.SetStyleCopFile((UniversalPath)newFilePath);
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog(StyleCopIgnoreUtility.UtilityName + " - Error", e.Message, "OK");
                    }
                }
            }

            this.indentLevel--;
            EditorGUILayout.EndHorizontal();
        }

        private void OnGUIFileTree()
        {
            EditorGUILayout.BeginVertical();
            var treeViewStyle = new GUIStyle(GUI.skin.FindStyle("CurveEditorBackground"));
            this.treeViewScrollPosition = EditorGUILayout.BeginScrollView(this.treeViewScrollPosition, treeViewStyle);

            EditorGUIUtility.SetIconSize(Vector2.one * 16);
            this.indentLevel--;
            this.OnGUIDirectory(this.fileTreeInfo.Root);
            EditorGUIUtility.SetIconSize(Vector2.zero);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void OnGUIDirectory(DirectoryView directoryView)
        {
            this.indentLevel++;

            EditorGUILayout.BeginHorizontal();

            ////===========  Foldout Toggle (change label/field width + wacky indent/spacing  ======================
            this.ManualGUIIndent();

            GUIStyle g = new GUIStyle(EditorStyles.foldout);
            g.fixedWidth = 16.0f;

            var previousFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 16.0f;
            directoryView.IsExpanded = EditorGUILayout.Foldout(directoryView.IsExpanded, string.Empty, g);
            EditorGUIUtility.fieldWidth = previousFieldWidth;

            EditorGUI.showMixedValue = directoryView.IsPartiallySelected;
            var isDirectoryIgnored = directoryView.IsSelected;
            var wantsToIgnoreFolder = EditorGUILayout.ToggleLeft(directoryView.Label, isDirectoryIgnored);
            if (wantsToIgnoreFolder != isDirectoryIgnored)
            {
                directoryView.ToggleSelectAll();
            }

            EditorGUI.showMixedValue = false;

            ////====================================================

            EditorGUILayout.EndHorizontal();

            if (directoryView.IsExpanded)
            {
                foreach (var subDirectory in directoryView.SubDirectories)
                {
                    this.OnGUIDirectory(subDirectory);
                }

                this.indentLevel += 2;
                foreach (var file in directoryView.Files)
                {
                    EditorGUILayout.BeginHorizontal();
                    this.ManualGUIIndent();
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    file.IsSelected = EditorGUILayout.ToggleLeft(file.Label, file.IsSelected, labelStyle);
                    EditorGUILayout.EndHorizontal();
                }

                this.indentLevel -= 2;
            }

            this.indentLevel--;
        }

        private void OnGUIStatusBar()
        {
            var errorMessage = this.GetStatusMessage();

            if (errorMessage == null)
            {
                return;
            }

            var labelStyle = new GUIStyle(EditorStyles.helpBox);
            labelStyle.alignment = TextAnchor.UpperLeft;
            labelStyle.padding = new RectOffset(2, 2, 2, 2);

            EditorGUIUtility.SetIconSize(Vector2.one * 16);
            EditorGUILayout.LabelField(errorMessage, labelStyle, GUILayout.Height(20));
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        private void OnGUIFooter()
        {
            EditorGUILayout.BeginHorizontal();

            bool isClearButtonDisabled = !this.styleCopIgnoreUtility.CachedIsStyleCopSettingsFileValid ||
                                         XMLTool.IgnoreListStorageMode == XMLTool.IgnoreListType.GeneratedFileFilter;
            EditorGUI.BeginDisabledGroup(isClearButtonDisabled);
            if (GUILayout.Button("Clear StyleCop"))
            {
                this.styleCopIgnoreUtility.OnUnloadFromStyleCop();
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            bool isSaveButtonDisabled = XMLTool.IgnoreListStorageMode == XMLTool.IgnoreListType.GeneratedFileFilter;
            EditorGUI.BeginDisabledGroup(isSaveButtonDisabled);
            if (GUILayout.Button("Save"))
            {
                this.styleCopIgnoreUtility.OnSave();
            }

            EditorGUI.EndDisabledGroup();

            bool isLoadButtonDisabled = !this.styleCopIgnoreUtility.CachedIsStyleCopSettingsFileValid;
            EditorGUI.BeginDisabledGroup(isLoadButtonDisabled);
            if (GUILayout.Button("Save and Load to StyleCop"))
            {
                this.styleCopIgnoreUtility.OnSave();
                this.styleCopIgnoreUtility.OnLoadToStyleCop();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private GUIContent GetStatusMessage()
        {
            if (!this.styleCopIgnoreUtility.CachedIsStyleCopSettingsFileValid)
            {
                return new GUIContent(
                    "StyleCop Settings file is not set, or it is invalid.",
                    EditorGUIUtility.IconContent("console.warnicon").image);
            }

            if (!this.styleCopIgnoreUtility.CachedIsStyleCopSynced)
            {
                return new GUIContent(
                    "StyleCop is not in sync.",
                    EditorGUIUtility.IconContent("console.warnicon").image);
            }

            return null;
        }

        private void ManualGUIIndent()
        {
            GUILayout.Space(this.indentLevel * 20.0f);
        }
    }
}