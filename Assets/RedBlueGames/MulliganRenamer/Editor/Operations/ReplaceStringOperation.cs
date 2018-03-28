/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class ReplaceStringOperation : RenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceStringOperation"/> class.
        /// </summary>
        public ReplaceStringOperation()
        {
            this.UseRegex = false;
            this.SearchString = string.Empty;
            this.SearchIsCaseSensitive = false;
            this.ReplacementString = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceStringOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ReplaceStringOperation(ReplaceStringOperation operationToCopy)
        {
            this.UseRegex = operationToCopy.UseRegex;
            this.SearchString = operationToCopy.SearchString;
            this.SearchIsCaseSensitive = operationToCopy.SearchIsCaseSensitive;
            this.ReplacementString = operationToCopy.ReplacementString;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Replace/Replace String";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ReplaceStringOperation"/>
        /// uses a regex expression for input.
        /// </summary>
        /// <value><c>true</c> if input is a regular expression; otherwise, <c>false</c>.</value>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets the search string that will be replaced.
        /// </summary>
        /// <value>The search string.</value>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case sensitive.
        /// </summary>
        /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
        public bool SearchIsCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the replacement string, which replaces instances of the search token.
        /// </summary>
        /// <value>The replacement string.</value>
        public string ReplacementString { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has errors that prevent it from Renaming.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public override bool HasErrors
        {
            get
            {
                if (this.UseRegex)
                {
                    return !IsValidRegex(this.ReplacementString)
                    || !IsValidRegex(this.SearchString);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return "Replace String";
            }
        }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public override Color32 HighlightColor
        {
            get
            {
                return this.ReplaceColor;
            }
        }

        /// <summary>
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public override string ControlToFocus
        {
            get
            {
                return "Search String";
            }
        }

        private string SearchRegexPattern
        {
            get
            {
                if (this.UseRegex)
                {
                    return this.SearchString;
                }
                else
                {
                    string searchStringRegexPattern = string.Empty;

                    if (!string.IsNullOrEmpty(this.SearchString))
                    {
                        // Escape the non-regex search string to prevent any embedded patterns from being interpretted as regex.
                        searchStringRegexPattern = string.Concat(Regex.Escape(this.SearchString));
                    }

                    return searchStringRegexPattern;
                }
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override RenameOperation Clone()
        {
            var clone = new ReplaceStringOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override RenameResult Rename(string input, int relativeCount)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new RenameResult();
            }

            RenameResult renameResult;
            if (string.IsNullOrEmpty(this.SearchString))
            {
                renameResult = new RenameResult();
                renameResult.Add(new Diff(input, DiffOperation.Equal));
                return renameResult;
            }

            MatchCollection matches;
            try
            {
                // Regex gives us case sensitivity, even when not searching with regex.
                var regexOptions = this.SearchIsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                matches = Regex.Matches(input, this.SearchRegexPattern, regexOptions);
            }
            catch (System.ArgumentException)
            {
                renameResult = new RenameResult();
                renameResult.Add(new Diff(input, DiffOperation.Equal));
                return renameResult;
            }

            renameResult = this.CreateDiffFromMatches(input, this.ReplacementString, matches);
            return renameResult;
        }

        protected override float GetPreferredHeightForContents()
        {
            var defaultHeight = this.CalculateHeightForGUILines(4);
            var preferredHeight = defaultHeight;
            if (this.HasErrors)
            {
                if (!IsValidRegex(this.SearchRegexPattern))
                {
                    preferredHeight += this.GetHeightForHelpBox();
                }

                if (!IsValidRegex(this.ReplacementString))
                {
                    preferredHeight += this.GetHeightForHelpBox();
                }
            }

            return preferredHeight;
        }

        private float GetHeightForHelpBox()
        {
            return 34.0f;
        }

        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var preGUIModel = new ReplaceStringOperation(this);
            var postGUIModel = new ReplaceStringOperation(preGUIModel);
            var weights = new List<float>(4);
            for (int i = 0; i < 4; ++i)
            {
                weights.Add(1.0f);
            }

            if (this.HasErrors)
            {
                if (!IsValidRegex(this.SearchRegexPattern))
                {
                    weights.Add(2.0f);
                }

                if (!IsValidRegex(this.ReplacementString))
                {
                    weights.Add(2.0f);
                }
            }

            var weightsArray = weights.ToArray();

            int currentGUIElement = 0;
            var regexToggleContent = new GUIContent("Use Regular Expression", "Match terms using Regular Expressions, terms that allow for powerful pattern matching.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, regexToggleContent.text));
            postGUIModel.UseRegex = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray),
                regexToggleContent,
                preGUIModel.UseRegex);

            GUIContent searchContent;
            GUIContent replacementContent;
            if (preGUIModel.UseRegex)
            {
                searchContent = new GUIContent("Match Regex", "Regular Expression to use to match terms.");
                replacementContent = new GUIContent("Replacement Regex", "Regular Expression to use when replacing matched patterns.");
            }
            else
            {
                searchContent = new GUIContent(
                    "Search for String",
                    "Substrings to search for in the filenames. These strings will be replaced by the Replacement String.");
                replacementContent = new GUIContent(
                    "Replace with",
                    "String to replace matching instances of the Search string.");
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Search String"));
            postGUIModel.SearchString = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray),
                searchContent,
                preGUIModel.SearchString);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Replacement String"));
            postGUIModel.ReplacementString = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray),
                replacementContent,
                preGUIModel.ReplacementString);

            var caseSensitiveContent = new GUIContent(
                                           "Case Sensitive",
                                           "Search using case sensitivity. Only strings that match the supplied casing will be replaced.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, caseSensitiveContent.text));
            postGUIModel.SearchIsCaseSensitive = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray),
                caseSensitiveContent,
                preGUIModel.SearchIsCaseSensitive);

            if (preGUIModel.HasErrors)
            {
                if (!IsValidRegex(preGUIModel.SearchRegexPattern))
                {
                    var helpRect = operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray);
                    helpRect = helpRect.AddPadding(4, 4, 4, 4);
                    EditorGUI.HelpBox(helpRect, "Match Expression is not a valid Regular Expression.", MessageType.Error);
                }

                if (!IsValidRegex(preGUIModel.ReplacementString))
                {
                    var helpRect = operationRect.GetSplitVerticalWeighted(++currentGUIElement, RenameOperation.LineSpacing, weightsArray);
                    helpRect = helpRect.AddPadding(4, 4, 4, 4);
                    EditorGUI.HelpBox(helpRect, "Replacement Expression is not a valid Regular Expression.", MessageType.Error);
                }
            }

            // Apply model back to this version to be represented next frame.
            this.CopyFrom(postGUIModel);
        }

        private void CopyFrom(ReplaceStringOperation other)
        {
            this.UseRegex = other.UseRegex;
            this.SearchString = other.SearchString;
            this.SearchIsCaseSensitive = other.SearchIsCaseSensitive;
            this.ReplacementString = other.ReplacementString;
        }

        private static bool IsValidRegex(string pattern)
        {
            // We consider empty a valid regular expression since Rename handles it gracefully
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            try
            {
                Regex.Match(string.Empty, pattern);
            }
            catch (System.ArgumentException)
            {
                return false;
            }

            return true;
        }

        private RenameResult CreateDiffFromMatches(string originalName, string replacementRegex, MatchCollection matches)
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
                var result = match.Result(replacementRegex);
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
    }
}