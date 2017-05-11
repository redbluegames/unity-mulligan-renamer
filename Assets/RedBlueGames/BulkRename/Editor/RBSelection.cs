using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// RBSelection adds funcitonality to the Selection static class. It monitors selected objects and stores them
/// in the order in which they were selected, instead of an arbitrary order.
/// </summary>
[InitializeOnLoad]
public static class RBSelection
{
    private static List<Object> selectedObjectsSortedByTime;

    /// <summary>
    /// Gets the selected objects sorted by time they were selected.
    /// </summary>
    /// <value>The selected objects sorted by time.</value>
    public static List<Object> SelectedObjectsSortedByTime
    {
        get
        {
            // Return a copy so they can't manipulate my list
            return new List<Object>(selectedObjectsSortedByTime);
        }
    }

    static RBSelection()
    {
        Selection.selectionChanged += RefreshObjectsToRename;
    }

    private static void RefreshObjectsToRename()
    {
        if (selectedObjectsSortedByTime == null)
        {
            selectedObjectsSortedByTime = new List<Object>();
        }

        // Get the selection diff
        var unitySelectedObjects = new List<Object>(Selection.objects);
        var addedObjects = GetObjectsInBNotInA(selectedObjectsSortedByTime, unitySelectedObjects);
        var removedObjects = GetObjectsInBNotInA(unitySelectedObjects, selectedObjectsSortedByTime);

        // Remove newly unselected objects
        foreach (var removedObject in removedObjects)
        {
            selectedObjectsSortedByTime.Remove(removedObject);
        }

        // Add newly selected objects
        foreach (var addedObject in addedObjects)
        {
            selectedObjectsSortedByTime.Add(addedObject);
        }
    }

    private static List<Object> GetObjectsInBNotInA(List<Object> listA, List<Object> listB)
    {
        var newObjects = new List<Object>();
        foreach (var obj in listB)
        {
            if (!listA.Contains(obj))
            {
                newObjects.Add(obj);
            }
        }

        return newObjects;
    }
}
