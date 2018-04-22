namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class StubRenameOpDrawer : RenameOperationDrawer<IRenameOperation>
    {
        public override string HeadingLabel
        {
            get
            {
                return "Hello World";
            }
        }

        public override Color32 HighlightColor
        {
            get
            {
                return Color.black;
            }
        }

        public override string MenuDisplayPath
        {
            get
            {
                return "Test";
            }
        }

        public override string ControlToFocus
        {
            get
            {
                return "";
            }
        }

        protected override float GetPreferredHeightForContents()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            GUI.Label(operationRect, this.HeadingLabel);
        }
    }
}