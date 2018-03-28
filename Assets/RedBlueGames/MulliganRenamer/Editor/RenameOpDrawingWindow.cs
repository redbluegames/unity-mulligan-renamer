using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RedBlueGames.MulliganRenamer;

public class RenameOpDrawingWindow : EditorWindow
{
    private Vector2 scrollPosition;

    public List<RenameOperation> RenameOperationPrototypes { get; private set; }

    private List<RenameOperation> operationClones = new List<RenameOperation>();

    [MenuItem("Window/TESTRneameOpDrawing", false)]
    private static void ShowRenameSpritesheetWindow()
    {
        EditorWindow.GetWindow<RenameOpDrawingWindow>(true, "OpDrawer", true);
    }

    private void OnEnable()
    {
        this.CacheRenameOperationPrototypes();

        foreach (var operationPrototype in this.RenameOperationPrototypes)
        {
            var clone = operationPrototype.Clone();
            this.operationClones.Add(clone);
        }
    }

    private void CacheRenameOperationPrototypes()
    {
        this.RenameOperationPrototypes = new List<RenameOperation>();

        this.RenameOperationPrototypes.Add(new AddStringOperation());
        this.RenameOperationPrototypes.Add(new ChangeCaseOperation());
        this.RenameOperationPrototypes.Add(new EnumerateOperation());
        this.RenameOperationPrototypes.Add(new RemoveCharactersOperation());
        this.RenameOperationPrototypes.Add(new ReplaceNameOperation());
        this.RenameOperationPrototypes.Add(new TrimCharactersOperation());
        this.RenameOperationPrototypes.Add(new ReplaceStringOperation());
    }

    private void OnGUI()
    {
        var operationRects = new List<Rect>();
        var totalPreferedHeight = 0.0f;
        var padding = new Vector2(4.0f, 4.0f);
        var spaccing = new Vector2(2.0f, 2.0f);

        foreach (var operation in this.operationClones)
        {
            var rect = new Rect(0.0f, totalPreferedHeight, this.position.width, operation.GetPreferredHeight());
            rect.position += spaccing;
            rect.size -= padding;

            operationRects.Add(rect);
            totalPreferedHeight += rect.height;
        }

        scrollPosition = GUI.BeginScrollView(
            new Rect(0.0f, 0.0f, this.position.width, this.position.height),
            this.scrollPosition,
            new Rect(0.0f, 0.0f, this.position.width, totalPreferedHeight));

        for (int i = 0; i < operationRects.Count; ++i)
        {
            var operation = this.operationClones[i];
            var rect = operationRects[i];
            operation.DrawGUI(rect, new RenameOperation.GUIOptions());
        }

        GUI.EndScrollView();
    }
}
