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
        //// Consts =============================================================================================================

        //// Fields =============================================================================================================

        private StyleCopIgnoreUtility styleCopIgnoreUtility;
        private FileTreeView fileTreeInfo;
        private Vector2 treeViewScrollPosition;
        private int indentLevel;

        //// Constructors =======================================================================================================

        private StyleCopIgnoreUtilityWindow()
        {
            this.titleContent = new GUIContent(StyleCopIgnoreUtility.WindowName);
            this.minSize = new Vector2(275, 300);

            this.fileTreeInfo = new FileTreeView((UniversalPath)Application.dataPath);
        }

        //// Properties =========================================================================================================

        //// Methods ============================================================================================================

        void IStyleCopIgnoreUtilityView.SetUtility(StyleCopIgnoreUtility utility)
        {
            this.styleCopIgnoreUtility = utility;
        }

        void IStyleCopIgnoreUtilityView.ReadPreferences(Preferences settings)
        {
            this.position = settings.WindowPosition;
        }

        void IStyleCopIgnoreUtilityView.WritePreferences(Preferences settings)
        {
            this.styleCopIgnoreUtility.UserPreferences.WindowPosition = this.position;
        }

        List<UniversalPath> IStyleCopIgnoreUtilityView.GetSelectedFiles()
        {
            return this.fileTreeInfo.GetSelectedFiles();
        }

        bool IStyleCopIgnoreUtilityView.InitializeWithData(StyleCopIgnoreUtilityData saveData)
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
            this.treeViewScrollPosition = EditorGUILayout.BeginScrollView(this.treeViewScrollPosition, EditorStyles.textArea);

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

            ////===========  Toggle Foldout (simpler)  ======================
            /*EditorGUI.showMixedValue = directoryView.IsPartiallyIgnored;
            GUILayout.Space( indent * 20 );
            EditorGUILayout.ToggleLeft( "", directoryView.IsIgnored, GUILayout.Width( 12.0f ) );
            EditorGUI.showMixedValue = false;
            directoryView.isExpanded = EditorGUILayout.Foldout( directoryView.isExpanded, directoryView.Label );*/
            ////====================================================

            ////===========  Foldout Toggle (change label/field width + wacky indent/spacing  ======================
            this.ManualGUIIndent();

            GUIStyle g = new GUIStyle(EditorStyles.foldout);
            g.fixedWidth = 16.0f;
            ////g.imagePosition = ImagePosition.ImageOnly;

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

                ////this.indentLevel++;
                this.indentLevel += 2;
                foreach (var file in directoryView.Files)
                {
                    EditorGUILayout.BeginHorizontal();
                    this.ManualGUIIndent();
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    file.IsSelected = EditorGUILayout.ToggleLeft(file.Label, file.IsSelected, labelStyle);
                    EditorGUILayout.EndHorizontal();
                }
                ////this.indentLevel--;
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