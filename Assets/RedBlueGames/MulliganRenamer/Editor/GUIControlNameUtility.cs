namespace RedBlueGames.MulliganRenamer
{
    public static class GUIControlNameUtility
    {
        private static readonly char Delimeter = '|';

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

        public static string CreatePrefixedName(int prefix, string controlName)
        {
            return string.Concat(prefix.ToString(), Delimeter, controlName);
        }
    }

}