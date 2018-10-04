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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool that helps us export RedBlueTools for use in other projects, as well as for the public
/// </summary>
public class RBPackageExporter : UnityEditor.EditorWindow
{
    private static string companyPath = "Assets/RedBlueGames";
    private static string packageExtension = ".unitypackage";
    private static string testPackageSuffix = "WithTests";

    private string pathToSettingsFile;
    private string pathToSettingsOverrideFile;

    private List<RBAsset> redBlueAssets;
    private List<RBAsset> selectedAssets;

    private bool includeTestFiles;
    private bool flagAsGithubRelease;

    [MenuItem("Assets/Red Blue/RBPackage Exporter")]
    private static void ExportRBScriptsWithTests()
    {
        EditorWindow.GetWindow<RBPackageExporter>(false, "RBPackage Exporter", true);
    }

    private static List<string> GetTestDirectories(string[] directories)
    {
        var testDirectories = new List<string>();
        foreach (var directory in directories)
        {
            if (IsDirectoryAChildOfAnyOfThese(directory, testDirectories))
            {
                testDirectories.Add(directory);
                continue;
            }

            if (IsTestDirectory(directory))
            {
                testDirectories.Add(directory);
            }
        }

        return testDirectories;
    }

    private static bool IsDirectoryAChildOfAnyOfThese(string path, List<string> possibleParentDirectories)
    {
        foreach (var possibleParentDirectory in possibleParentDirectories)
        {
            if (path.Contains(possibleParentDirectory))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTestDirectory(string path)
    {
        return System.IO.Path.GetFileName(path) == "Tests";
    }

    private void OnEnable()
    {
        this.pathToSettingsFile = System.IO.Path.Combine(Application.dataPath, "RedBlueGames/MulliganRenamer/Editor/RBPackageSettings.cs");
        this.pathToSettingsOverrideFile =
            Application.dataPath +
            System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar +
            "RBPackageSettings.cs";

        this.redBlueAssets = new List<RBAsset>();
        this.selectedAssets = new List<RBAsset>();

        this.FindAssetsInCompanyFolder();
    }

    private void FindAssetsInCompanyFolder()
    {
        foreach (var subdirectory in System.IO.Directory.GetDirectories(companyPath))
        {
            var splitSubdirectory = subdirectory.Split(System.IO.Path.DirectorySeparatorChar);
            string folderName = splitSubdirectory[splitSubdirectory.Length - 1];
            this.redBlueAssets.Add(new RBAsset()
            {
                AssetName = folderName,
                IsSelected = false
            });
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "This tool Allows quick export of specific RedBlueGames custom assets. It also allows optional export of Tests folders.",
            MessageType.None);
        EditorGUILayout.LabelField("Asset Packages to export:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < this.redBlueAssets.Count; i++)
        {
            this.redBlueAssets[i].IsSelected = EditorGUILayout.Toggle(
                this.redBlueAssets[i].AssetName,
                this.redBlueAssets[i].IsSelected);
        }

        EditorGUILayout.EndVertical();

        // Check if any assets are selected.
        this.selectedAssets.Clear();
        foreach (var assetPackage in this.redBlueAssets)
        {
            if (assetPackage.IsSelected)
            {
                this.selectedAssets.Add(assetPackage);
            }
        }

        bool atLeastOnePackageSelected = this.selectedAssets.Count > 0;

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Additional Options:", EditorStyles.boldLabel);
        this.includeTestFiles = EditorGUILayout.Toggle("Include Test Files", this.includeTestFiles);

        this.flagAsGithubRelease = EditorGUILayout.Toggle("Flag as GitHub relase", this.flagAsGithubRelease);

        EditorGUI.BeginDisabledGroup(!atLeastOnePackageSelected);
        if (GUILayout.Button("Export"))
        {
            string oldSettings = string.Empty;
            if (this.flagAsGithubRelease)
            {
                oldSettings = System.IO.File.ReadAllText(pathToSettingsFile);
                var settingsOverride = System.IO.File.ReadAllText(pathToSettingsOverrideFile);
                settingsOverride = string.Concat("/* THIS CLASS IS AUTO-GENERATED! DO NOT MODIFY!  */ \n", settingsOverride);
                System.IO.File.WriteAllText(this.pathToSettingsFile, settingsOverride);
            }

            this.ExportPackages(this.selectedAssets, this.includeTestFiles);

            // Restore the old file so that we don't dirty the repo.
            if (!string.IsNullOrEmpty(oldSettings))
            {
                System.IO.File.WriteAllText(this.pathToSettingsFile, oldSettings);
            }
        }

        EditorGUI.EndDisabledGroup();

        if (!atLeastOnePackageSelected)
        {
            EditorGUILayout.HelpBox(
                "No packages selected to export. Select at least one asset Package.",
                MessageType.Warning);
        }
    }

    private void HandleTestsFailed()
    {
        string dialogTitle = "Export Error";
        string exportErrorMsg = "Could not export packages because the Unit tests failed. " +
            "You must fix the tests before exporting a project.";
        string confirmButtonText = "OK";
        UnityEditor.EditorUtility.DisplayDialog(dialogTitle, exportErrorMsg, confirmButtonText);
    }

    private void ExportPackages(List<RBAsset> packages, bool includeTests)
    {
        // We can only select the exported package (runInterative) if there's one (Unity crashes otherwise).
        bool runInteractive = packages.Count == 1;
        foreach (var asset in packages)
        {
            this.ExportRBScripts(asset, includeTests, runInteractive);
        }
    }

    private void ExportRBScripts(RBAsset assetToExport, bool includeTests, bool runInteractive)
    {
        var subDirectories = System.IO.Directory.GetDirectories(companyPath, "*", System.IO.SearchOption.AllDirectories);
        var directoriesToExport = new List<string>(subDirectories);

        var testDirectories = GetTestDirectories(subDirectories);
        if (!includeTests)
        {
            foreach (var testDirectory in testDirectories)
            {
                directoriesToExport.Remove(testDirectory);
            }
        }

        // Do not export the other packages
        foreach (var asset in this.redBlueAssets)
        {
            if (assetToExport.AssetName != asset.AssetName)
            {
                string assetPath = companyPath + System.IO.Path.DirectorySeparatorChar + asset.AssetName;
                directoriesToExport.Remove(assetPath);

                var subdirectoriesOfAsset = System.IO.Directory.GetDirectories(assetPath, "*", System.IO.SearchOption.AllDirectories);
                foreach (var subdirectory in subdirectoriesOfAsset)
                {
                    directoriesToExport.Remove(subdirectory);
                }
            }
        }

        var allAssetPaths = new List<string>();
        foreach (var directory in directoriesToExport)
        {
            var filesInDirectory = System.IO.Directory.GetFiles(directory);
            allAssetPaths.AddRange(filesInDirectory);
        }

        if (allAssetPaths.Count == 0)
        {
            Debug.Log("No assets to export. Will not export asset package: " + assetToExport.AssetName);
            return;
        }

        string filename = string.Concat(assetToExport.AssetName, includeTests ? testPackageSuffix : string.Empty, packageExtension);
        ExportPackageOptions exportOptions;
        if (runInteractive)
        {
            exportOptions = ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Interactive;
        }
        else
        {
            exportOptions = ExportPackageOptions.IncludeDependencies;
        }

        AssetDatabase.ExportPackage(allAssetPaths.ToArray(), filename, exportOptions);
    }

    private class RBAsset
    {
        public string AssetName { get; set; }

        public bool IsSelected { get; set; }
    }
}