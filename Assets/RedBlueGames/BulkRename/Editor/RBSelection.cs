namespace RedBlueGames.BulkRename
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// RBSelection adds funcitonality to the Selection static class. It monitors selected objects and stores them
    /// in the order in which they were selected, instead of an arbitrary order.
    /// </summary>
    [InitializeOnLoad]
    public static class RBSelection
    {
        private static List<UnityObject> selectedObjectsSortedByTime;

        static RBSelection()
        {
            selectedObjectsSortedByTime = new List<UnityObject>();
            Selection.selectionChanged += RefreshObjectsToRename;
        }

        /// <summary>
        /// Gets the selected objects sorted by time they were selected.
        /// </summary>
        /// <value>The selected objects sorted by time.</value>
        public static List<UnityObject> SelectedObjectsSortedByTime
        {
            get
            {
                // Return a copy so they can't manipulate my list
                return new List<UnityObject>(selectedObjectsSortedByTime);
            }
        }

        private static void RefreshObjectsToRename()
        {
            // Get the selection diff
            var unitySelectedObjects = new List<UnityObject>(Selection.objects);
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

        private static List<UnityObject> GetObjectsInBNotInA(List<UnityObject> listA, List<UnityObject> listB)
        {
            var newObjects = new List<UnityObject>();
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
}