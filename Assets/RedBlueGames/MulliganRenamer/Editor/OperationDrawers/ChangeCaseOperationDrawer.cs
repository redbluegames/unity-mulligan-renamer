namespace RedBlueGames.MulliganRenamer
{
    using UnityEngine;
    using UnityEditor;

    public class ChangeCaseOperationDrawer : RenameOperationDrawer<ChangeCaseOperation>
    {
        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Modify/Change Case";
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
                return "Change Case";
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
                return this.ModifyColor;
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
                return "To Uppercase";
            }
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(1);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            // Split even a single line GUI so that we properly subtract out spacing
            var singleLineRect = operationRect.GetSplitVertical(1, 1, LineSpacing);

            var casingLabel = new GUIContent("New Casing", "The desired casing for the new name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "To Uppercase"));
            this.RenameOperation.Casing = (ChangeCaseOperation.CasingChange)EditorGUI.EnumPopup(
                singleLineRect,
                casingLabel,
                this.RenameOperation.Casing);
        }
    }
}