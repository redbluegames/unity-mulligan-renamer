namespace RedBlueGames.StyleCopIgnoreUtility
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes what a View should contain in order to interface with this Utility.
    /// </summary>
    internal interface IStyleCopIgnoreUtilityView
    {
        /// <summary>
        /// Sets a reference to the Utility in order to communicate with it
        /// </summary>
        /// <param name="utility">The StyleCopIgnoreUtility</param>
        void SetUtility(StyleCopIgnoreUtility utility);

        /// <summary>
        /// Initialize the View with the provided saved data so that the View can reflect the data's current state
        /// </summary>
        /// <param name="data">The saved data to represent</param>
        /// <returns>Returns true if the operation suceeded, otherwise false</returns>
        bool InitializeWithData(StyleCopIgnoreUtilityData data);

        /// <summary>
        /// Reads and applies user preferences to customize the View
        /// </summary>
        /// <param name="preferences">User preferences to read from</param>
        void ReadPreferences(Preferences preferences);

        /// <summary>
        /// Saves user preferences to customize the View
        /// </summary>
        /// <param name="preferences">User preferences to save to</param>
        void WritePreferences(Preferences preferences);

        /// <summary>
        /// Gets all of the files that are currently selected ('checked') in the View
        /// </summary>
        /// <returns>Returns a List of strings containing complete, full file paths (including the filename and extension)</returns>
        List<UniversalPath> GetSelectedFiles();
    }
}