namespace RedBlueGames.StyleCopIgnoreUtility
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// User preferences for the Utility
    /// </summary>
    internal class Preferences
    {
        //// Consts =============================================================================================================

        private const string PrefNameWindowX = StyleCopIgnoreUtility.UtilityName + "x";
        private const string PrefNameWindowY = StyleCopIgnoreUtility.UtilityName + "y";
        private const string PrefNameWindowWidth = StyleCopIgnoreUtility.UtilityName + "w";
        private const string PrefNameWindowHeight = StyleCopIgnoreUtility.UtilityName + "h";

        private const int DefaultWindowWidth = 500;
        private const int DefaultWindowHeight = 600;

        //// Fields =============================================================================================================

        //// Constructors =======================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="Preferences"/> class
        /// </summary>
        internal Preferences()
        {
            this.WindowPosition = new Rect(0, 0, DefaultWindowWidth, DefaultWindowHeight);
        }

        //// Properties =========================================================================================================
        
        /// <summary>
        /// Gets or sets the size and position of the utility window
        /// </summary>
        internal Rect WindowPosition { get; set; }

        //// Methods ============================================================================================================

        /// <summary>
        /// Loads user preferences from Unity's EditorPrefs
        /// </summary>
        internal void Load()
        {
            var savedWindowPosition = new Rect(this.WindowPosition);

            if (EditorPrefs.HasKey(PrefNameWindowX))
            {
                savedWindowPosition.x = EditorPrefs.GetInt(PrefNameWindowX);
            }

            if (EditorPrefs.HasKey(PrefNameWindowY))
            {
                savedWindowPosition.y = EditorPrefs.GetInt(PrefNameWindowY);
            }

            if (EditorPrefs.HasKey(PrefNameWindowWidth))
            {
                savedWindowPosition.width = EditorPrefs.GetInt(PrefNameWindowWidth);
            }

            if (EditorPrefs.HasKey(PrefNameWindowHeight))
            {
                savedWindowPosition.height = EditorPrefs.GetInt(PrefNameWindowHeight);
            }

            this.WindowPosition = savedWindowPosition;
        }

        /// <summary>
        /// Saves user preferences to Unity's EditorPrefs
        /// </summary>
        internal void Save()
        {
            EditorPrefs.SetInt(PrefNameWindowX, (int)this.WindowPosition.x);
            EditorPrefs.SetInt(PrefNameWindowY, (int)this.WindowPosition.y);
            EditorPrefs.SetInt(PrefNameWindowWidth, (int)this.WindowPosition.width);
            EditorPrefs.SetInt(PrefNameWindowHeight, (int)this.WindowPosition.height);
        }
    }
}