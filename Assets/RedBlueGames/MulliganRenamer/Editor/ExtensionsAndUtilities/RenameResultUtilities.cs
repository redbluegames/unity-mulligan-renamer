namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Static functions that help manipulate or create RenameResults
    /// </summary>
    public static class RenameResultUtilities
    {
        public delegate string ReplaceMatchFunction(Match match);

        /// <summary>
        /// Compares two strings and constructs a diff based on their differences. It simply compares character by character,
        /// no analysis is done to see if a character could have moved.
        /// </summary>
        /// <param name="originalString">The original string. Letters in here that aren't in the new string will be deletions.</param>
        /// <param name="newString">The new string. Letters in here that aren't in the originalString will be insertions.</param>
        /// <returns>A diff sequence representing changes between the two strings</returns>
        public static RenameResult GetDiffResultFromStrings(string originalString, string newString)
        {
            var renameResult = new RenameResult();
            var consecutiveEqualChars = string.Empty;
            var longestLength = Mathf.Max(originalString.Length, newString.Length);
            for (int i = 0; i < longestLength; ++i)
            {
                if (i >= newString.Length)
                {
                    // Consolidate the diff with the remainder of the string so that we get a cleaner diff
                    // (ex: ABC => ABDog comes back as [AB=],[C-],[Dog+] instead of [AB=],[C-],[D+],[og+])
                    ConsolidateRemainderOfStringIntoRenameResult(renameResult, originalString, i, DiffOperation.Deletion);
                    break;
                }
                else if (i >= originalString.Length)
                {
                    // Consolidate the diff with the remainder of the string so that we get a cleaner diff
                    // (ex: ABC => ABDog comes back as [AB=],[C-],[Dog+] instead of [AB=],[C-],[D+],[og+])
                    ConsolidateRemainderOfStringIntoRenameResult(renameResult, newString, i, DiffOperation.Insertion);
                    break;
                }
                else
                {
                    string oldLetter = originalString.Substring(i, 1);
                    string newLetter = newString.Substring(i, 1);
                    if (oldLetter.Equals(newLetter))
                    {
                        consecutiveEqualChars = string.Concat(consecutiveEqualChars, oldLetter);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(consecutiveEqualChars))
                        {
                            renameResult.Add(new Diff(consecutiveEqualChars, DiffOperation.Equal));
                            consecutiveEqualChars = string.Empty;
                        }

                        renameResult.Add(new Diff(oldLetter, DiffOperation.Deletion));
                        renameResult.Add(new Diff(newLetter, DiffOperation.Insertion));
                    }
                }
            }

            if (!string.IsNullOrEmpty(consecutiveEqualChars))
            {
                renameResult.Add(new Diff(consecutiveEqualChars, DiffOperation.Equal));
            }

            return renameResult;
        }

        /// <summary>
        /// Creates a RenameResult (Diff) based on regex matches and a replacement function to replace each match.
        /// </summary>
        /// <param name="originalName">Original string to match against</param>
        /// <param name="replacementFunction">Replacement function, used to replace each match</param>
        /// <param name="matches">Matches from a regex Matches query</param>
        /// <returns>A diff of the original and resulting strings (original with Matches replaced using the replacement function)</returns>
        public static RenameResult CreateDiffFromReplacedMatches(string originalName, ReplaceMatchFunction replacementFunction, MatchCollection matches)
        {
            var renameResult = new RenameResult();
            var nextMatchStartingIndex = 0;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // Grab the substring before the match
                if (nextMatchStartingIndex < match.Index)
                {
                    string before = originalName.Substring(nextMatchStartingIndex, match.Index - nextMatchStartingIndex);
                    renameResult.Add(new Diff(before, DiffOperation.Equal));
                }

                // Add the match as a deletion
                renameResult.Add(new Diff(match.Value, DiffOperation.Deletion));

                // Add the result as an insertion
                var result = replacementFunction.Invoke(match);
                if (!string.IsNullOrEmpty(result))
                {
                    renameResult.Add(new Diff(result, DiffOperation.Insertion));
                }

                nextMatchStartingIndex = match.Index + match.Length;
            }

            if (nextMatchStartingIndex < originalName.Length)
            {
                var lastSubstring = originalName.Substring(nextMatchStartingIndex, originalName.Length - nextMatchStartingIndex);
                renameResult.Add(new Diff(lastSubstring, DiffOperation.Equal));
            }

            return renameResult;
        }

        private static void ConsolidateRemainderOfStringIntoRenameResult(
            RenameResult renameResult,
            string originalString,
            int indexToStartFrom,
            DiffOperation diffOpIfDifferent)
        {
            var remainderOfOldString = originalString.Substring(indexToStartFrom, originalString.Length - indexToStartFrom);
            if (renameResult.Count > 0 && renameResult[renameResult.Count - 1].Operation == diffOpIfDifferent)
            {
                // last diff in the sequence matches the desired diff for the remainder of the string. Consolidate them
                var previousDiff = renameResult[renameResult.Count - 1];
                renameResult[renameResult.Count - 1] = new Diff(
                    string.Concat(previousDiff.Result, remainderOfOldString),
                    previousDiff.Operation);
            }
            else
            {
                renameResult.Add(new Diff(remainderOfOldString, diffOpIfDifferent));
            }
        }
    }
}