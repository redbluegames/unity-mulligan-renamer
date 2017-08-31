using UnityEditor;
using UnityEngine;

/// <summary>
/// Extensions for the GameObject class
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Gets the hierarchy sorting for the GameObject. This is like Sibling index only weighted based on the
    /// position of the object in the hierarchy. It should match the way it displays in the Unity scene.
    /// </summary>
    /// <returns>The hierarchy sorting of the GameObject.</returns>
    /// <param name="obj">GameObject to get the sorting for.</param>
    public static float GetHierarchySorting(this GameObject obj)
    {
        if (obj.transform.parent == null)
        {
            return obj.transform.GetSiblingIndex();
        }
        else
        {
            // Sorting =  parent.Sorting + siblingWeight * (siblingIndex + 1)
            // A = 0
            //  - a1 0 + (0.34 * 1) = 0.34
            //    - a2 0.34 + (0.17 * 1) = 0.51
            //  - b1 0 + (0.34 * 2) = 0.64
            // B = 1
            float weightFromSiblingIndex = obj.transform.GetSiblingWeight() * (obj.transform.GetSiblingIndex() + 1);
            return obj.transform.parent.gameObject.GetHierarchySorting() + weightFromSiblingIndex;
        }
    }

    private static float GetSiblingWeight(this Transform t)
    {
        if (t.parent == null)
        {
            return 1;
        }
        else
        {
            // SiblingWeight is the product of the weight of the parent divided amongst their children.
            // An element with 5 siblings would have a sibling weight of .2.
            return t.parent.GetSiblingWeight() / (t.parent.childCount + 1);
        }
    }
}
