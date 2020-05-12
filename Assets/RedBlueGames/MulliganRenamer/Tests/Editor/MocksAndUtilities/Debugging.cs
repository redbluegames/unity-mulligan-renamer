namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class Debugging : MonoBehaviour
    {
        [MenuItem("Assets/Red Blue/Mulligan Renamer/Delete All Prefs")]
        public static void DeletePrefs()
        {
            var activePreferences = MulliganUserPreferences.LoadOrCreatePreferences();
            activePreferences.ResetToDefaults();
            activePreferences.SaveToEditorPrefs();
            MulliganUserPreferences.Debug_DeletePreferences();
            LocalizationManager.Instance.Initialize();
        }
    }
}