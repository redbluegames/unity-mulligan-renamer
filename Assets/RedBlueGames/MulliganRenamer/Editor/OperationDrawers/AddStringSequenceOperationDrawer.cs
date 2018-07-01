namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class AddStringSequenceOperationDrawer : RenameOperationDrawer<CountByLetterOperation>
    {
        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Add/String Sequence";
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
                return "Add String Sequence";
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
                return this.AddColor;
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
                return "Sequence";
            }
        }

        private int SelectedModeIndex { get; set; }

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
            // AddStringSequence is just a CountByLetter without carrying over
            this.RenameOperation.DoNotCarryOver = true;

            var currentRectSplit = 0;
            var numLines = 1;

            var stringRect = operationRect.GetSplitVertical(++currentRectSplit, numLines, LineSpacing);
            var stringSequence = this.DrawStringSequenceField(
                stringRect,
                controlPrefix,
                this.RenameOperation.CountSequence);
            if (stringSequence != null)
            {
                this.RenameOperation.CountSequence = stringSequence;
            }
        }

        private string[] DrawStringSequenceField(Rect rect, int controlPrefix, string[] stringSequence)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Sequence"));

            var sequenceContent = new GUIContent("Sequence", "The sequence of strings to add, comma separted.");
            var oldSequence = StringUtilities.AddCommasBetweenStrings(stringSequence);
            var sequenceStrings = oldSequence;
            var sequenceWithCommas = EditorGUI.TextField(
                rect,
                sequenceContent,
                sequenceStrings);

            return StringUtilities.StripCommasFromString(sequenceWithCommas);
        }
    }
}