namespace RedBlueGames.MulliganRenamer
{
    using UnityEngine;
    using UnityEditor;

    public class AddStringSequenceOperationDrawer : RenameOperationDrawer<AddStringSequenceOperation>
    {
        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Add/Add String Sequence";
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
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, "Sequence"));

            var content = new GUIContent("Sequence", "The sequence of strings to add, comma separted.");
            var oldSequence = AddComasBetweenStrings(this.RenameOperation.StringSequence);
            var sequenceWithCommas = EditorGUI.TextField(
                operationRect.GetSplitVertical(1, 1, LineSpacing),
                content,
                oldSequence);

            this.RenameOperation.StringSequence = StripComasFromString(sequenceWithCommas);
        }

        private static string[] StripComasFromString(string stringWithComas)
        {
            var splits = stringWithComas.Split(',');
            return splits;
        }

        private static string AddComasBetweenStrings(string[] strings)
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