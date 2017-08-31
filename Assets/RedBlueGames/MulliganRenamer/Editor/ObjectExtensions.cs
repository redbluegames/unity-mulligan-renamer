using UnityEditor;
using UnityEngine;

/// <summary>
/// Extension methods for UnityObject's.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Determines if the specified Unity object is an Asset
    /// </summary>
    /// <returns><c>true</c> if the object is an asset; otherwise, <c>false</c>.</returns>
    /// <param name="obj">Object to test.</param>
    public static bool IsAsset(this UnityEngine.Object obj)
    {
        return AssetDatabase.Contains(obj);
    }
}
