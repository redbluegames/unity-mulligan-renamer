namespace RedBlueGames.MulliganRenamer
{
    using UnityEngine;
    using UnityEditor;

    public class TrimCharactersOperationDrawer : RenameOperationDrawer<TrimCharactersOperation>
    {
        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Delete/Trim Characters";
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
                return "Trim Characters";
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
                return this.DeleteColor;
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
                return "Delete from Front";
            }
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(2);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Delete from Front"));
            this.RenameOperation.NumFrontDeleteChars = EditorGUI.IntField(
                operationRect.GetSplitVertical(1, 2, LineSpacing),
                "Delete from Front",
                this.RenameOperation.NumFrontDeleteChars);
            this.RenameOperation.NumFrontDeleteChars = Mathf.Max(0, this.RenameOperation.NumFrontDeleteChars);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Delete from Back"));
            this.RenameOperation.NumBackDeleteChars = EditorGUI.IntField(
                operationRect.GetSplitVertical(2, 2, LineSpacing),
                "Delete from Back",
                this.RenameOperation.NumBackDeleteChars);
            this.RenameOperation.NumBackDeleteChars = Mathf.Max(0, this.RenameOperation.NumBackDeleteChars);
        }
    }
}