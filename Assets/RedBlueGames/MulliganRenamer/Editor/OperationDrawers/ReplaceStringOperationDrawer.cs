namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class ReplaceStringOperationDrawer : RenameOperationDrawer<ReplaceStringOperation>
    {
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

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            var defaultHeight = this.CalculateGUIHeightForLines(4);
            var preferredHeight = defaultHeight;
            if (this.RenameOperation.HasErrors())
            {
                if (!this.RenameOperation.SearchStringIsValidRegex)
                {
                    preferredHeight += GetHeightForHelpBox();
                }

                if (!this.RenameOperation.ReplacementStringIsValidRegex)
                {
                    preferredHeight += GetHeightForHelpBox();
                }
            }

            return preferredHeight;
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var preGUIModel = (ReplaceStringOperation)this.RenameOperation.Clone();
            var postGUIModel = (ReplaceStringOperation)preGUIModel.Clone();
            var weights = new List<float>(4);
            for (int i = 0; i < 4; ++i)
            {
                weights.Add(1.0f);
            }

            if (preGUIModel.HasErrors())
            {
                if (!preGUIModel.SearchStringIsValidRegex)
                {
                    weights.Add(2.0f);
                }

                if (!preGUIModel.ReplacementStringIsValidRegex)
                {
                    weights.Add(2.0f);
                }
            }

            var weightsArray = weights.ToArray();

            int currentGUIElement = 0;
            var regexToggleContent = new GUIContent("Use Regular Expression", "Match terms using Regular Expressions, terms that allow for powerful pattern matching.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, regexToggleContent.text));
            postGUIModel.UseRegex = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
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
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                searchContent,
                preGUIModel.SearchString);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Replacement String"));
            postGUIModel.ReplacementString = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                replacementContent,
                preGUIModel.ReplacementString);

            var caseSensitiveContent = new GUIContent(
                                           "Case Sensitive",
                                           "Search using case sensitivity. Only strings that match the supplied casing will be replaced.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, caseSensitiveContent.text));
            postGUIModel.SearchIsCaseSensitive = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                caseSensitiveContent,
                preGUIModel.SearchIsCaseSensitive);

            if (preGUIModel.HasErrors())
            {

                if (!preGUIModel.SearchStringIsValidRegex)
                {
                    var helpRect = operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray);
                    helpRect = helpRect.AddPadding(4, 4, 4, 4);
                    EditorGUI.HelpBox(helpRect, "Match Expression is not a valid Regular Expression.", MessageType.Error);
                }

                if (!preGUIModel.ReplacementStringIsValidRegex)
                {
                    var helpRect = operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray);
                    helpRect = helpRect.AddPadding(4, 4, 4, 4);
                    EditorGUI.HelpBox(helpRect, "Replacement Expression is not a valid Regular Expression.", MessageType.Error);
                }
            }

            // Apply model back to this version to be represented next frame.
            this.RenameOperation.CopyFrom(postGUIModel);
        }

        private static float GetHeightForHelpBox()
        {
            return 34.0f;
        }
    }
}