namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class StringUtilities
    {
        public static string[] StripCommasFromString(string stringWithComas)
        {
            var splits = stringWithComas.Split(',');
            return splits;
        }

        public static string AddCommasBetweenStrings(string[] strings)
        {
            var fullString = string.Empty;
            for (int i = 0; i < strings.Length; ++i)
            {
                fullString = string.Concat(fullString, strings[i]);
                if (i < strings.Length - 1)
                {
                    fullString = string.Concat(fullString, ",");
                }
            }

            return fullString;
        }
    }
}