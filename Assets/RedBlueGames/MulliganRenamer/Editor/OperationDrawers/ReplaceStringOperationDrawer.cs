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
                return GetOperationPath("replace", "replaceString");
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
                return LocalizationManager.Instance.GetTranslation("replaceString");
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
                return LocalizationManager.Instance.GetTranslation("searchString");
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
            var regexToggleContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("useRegex"),
                LocalizationManager.Instance.GetTranslation("matchTermsUsingRegex"));
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, regexToggleContent.text));
            postGUIModel.UseRegex = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                regexToggleContent,
                preGUIModel.UseRegex);

            GUIContent searchContent;
            GUIContent replacementContent;
            if (preGUIModel.UseRegex)
            {
                searchContent = new GUIContent(
                    LocalizationManager.Instance.GetTranslation("matchRegex"),
                    LocalizationManager.Instance.GetTranslation("regexToUseToMatchTerms"));
                replacementContent = new GUIContent("Replacement Regex", "Regular Expression to use when replacing matched patterns.");
            }
            else
            {
                searchContent = new GUIContent(
                    LocalizationManager.Instance.GetTranslation("searchForString"),
                    LocalizationManager.Instance.GetTranslation("substringsToSeatchInFilenames"));
                replacementContent = new GUIContent(
                    LocalizationManager.Instance.GetTranslation("replaceWith"),
                    LocalizationManager.Instance.GetTranslation("stringToReplaceMatchingInstances"));
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, LocalizationManager.Instance.GetTranslation("searchString")));
            postGUIModel.SearchString = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                searchContent,
                preGUIModel.SearchString);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, LocalizationManager.Instance.GetTranslation("replacementString")));
            postGUIModel.ReplacementString = EditorGUI.TextField(
                operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray),
                replacementContent,
                preGUIModel.ReplacementString);

            var caseSensitiveContent = new GUIContent(
                LocalizationManager.Instance.GetTranslation("caseSensitive"),
                LocalizationManager.Instance.GetTranslation("searchUsingCaseSensitivity"));
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
                    EditorGUI.HelpBox(helpRect, LocalizationManager.Instance.GetTranslation("matchExpressNotValid"), MessageType.Error);
                }

                if (!preGUIModel.ReplacementStringIsValidRegex)
                {
                    var helpRect = operationRect.GetSplitVerticalWeighted(++currentGUIElement, LineSpacing, weightsArray);
                    helpRect = helpRect.AddPadding(4, 4, 4, 4);
                    EditorGUI.HelpBox(helpRect, LocalizationManager.Instance.GetTranslation("replacementExpressionNotValid"), MessageType.Error);
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