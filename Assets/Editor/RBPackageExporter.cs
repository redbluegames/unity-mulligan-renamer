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

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool that helps us export RedBlueTools for use in other projects, as well as for the public
/// </summary>
public class RBPackageExporter
{
    public void Export(bool includeTestDirectories)
    {
        // Export function heavily inspired by this blog post:
        // https://medium.com/@neuecc/using-circle-ci-to-build-test-make-unitypackage-on-unity-9f9fa2b3adfd
        var root = "RedBlueGames/MulliganRenamer";
        var exportPath = "./Mulligan_CI.unitypackage";

        var path = Path.Combine(Application.dataPath, root);
        var assets = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/")).ToArray();

        if (!includeTestDirectories)
        {
            assets = assets.Where(x => !x.Contains("Tests")).ToArray();
        }

        UnityEngine.Debug.Log("Exporting these files:" + Environment.NewLine + string.Join(Environment.NewLine, assets));

        AssetDatabase.ExportPackage(
            assets,
            exportPath,
            ExportPackageOptions.Default);

        UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
    }
}