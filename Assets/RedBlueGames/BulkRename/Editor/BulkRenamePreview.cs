using System.Collections.Generic;
using RedBlueGames.BulkRename.Dependencies.DiffMatchPatch;

namespace RedBlueGames.BulkRename
{
    public class BulkRenamePreview
    {
        public string OriginalName { get; private set; }

        public string NewName { get; private set; }

        private List<Diff> Diffs { get; set; }

        public BulkRenamePreview(string originalName, string newName)
        {
            this.OriginalName = originalName;
            this.NewName = newName;

            var differ = new diff_match_patch();
            this.Diffs = differ.diff_main(originalName, newName, false);
        }

        public string GetDiffAsFormattedString(string colorTagForAdds, string colorTagForDeletes)
        {
            var diffedName = string.Empty;
            foreach (var diffResult in this.Diffs)
            {
                var segment = string.Empty;
                if (diffResult.operation == Operation.DELETE)
                {
                    segment = string.Concat(colorTagForAdds, diffResult.text, "</color>");
                }
                else if (diffResult.operation == Operation.INSERT)
                {
                    segment = string.Concat(colorTagForDeletes, diffResult.text, "</color>");
                }
                else
                {
                    segment = diffResult.text;
                }

                diffedName += segment;
            }

            return diffedName;
        }
    }
}