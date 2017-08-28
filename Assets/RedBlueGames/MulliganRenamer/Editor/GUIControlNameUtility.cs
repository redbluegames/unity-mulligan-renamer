namespace RedBlueGames.MulliganRenamer
{
    /// <summary>
    /// GUI control name utility provides functions that help give unique names to controls that
    /// might have duplicates.
    /// </summary>
    public static class GUIControlNameUtility
    {
        private static readonly char Delimeter = '|';

        /// <summary>
        /// Gets the prefix of the control from a name that's been created with this utility.
        /// Names are of the format "#|Control" where # is the prefix value.
        /// </summary>
        /// <returns>The prefix in the name.</returns>
        /// <param name="name">Name that has a prefix.</param>
        public static int GetPrefixFromName(string name)
        {
            var split = name.Split(Delimeter);
            int prefix = -1;

            if (split.Length != 2)
            {
                var exception = string.Format(
                                    "Expected name of format '#|ControlName' but it did not parse correctly. Argument: {0}", name);
                throw new System.ArgumentException(exception);
            }

            int.TryParse(split[0], out prefix);
            return prefix;
        }

        /// <summary>
        /// Creates a prefixed name from a prefix and control name.
        /// Names are of the format "#|Control" where # is the prefix value.
        /// </summary>
        /// <returns>The prefixed name.</returns>
        /// <param name="prefix">Prefix to concatenate.</param>
        /// <param name="controlName">Control name.</param>
        public static string CreatePrefixedName(int prefix, string controlName)
        {
            return string.Concat(prefix.ToString(), Delimeter, controlName);
        }
    }
}