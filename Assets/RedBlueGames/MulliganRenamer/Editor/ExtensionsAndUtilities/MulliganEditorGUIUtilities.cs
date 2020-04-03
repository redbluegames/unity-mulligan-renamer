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
    using UnityEditor;
    using UnityEngine;

    public static class MulliganEditorGUIUtilities
    {
        /// <summary>
        /// Draw a DiffLabel, which draws a simple EditorGUILabel populated with the results from a rename op (diff).
        /// </summary>
        /// <param name="rect">Rect to draw in</param>
        /// <param name="renameResult">The result of a RenameOp, which contains the diffs to render</param>
        /// <param name="showBefore">Flag to show the name before the op, instead of the result</param>
        /// <param name="resultLabelStyle">Style of the DiffLabel</param>
        /// <param name="style">Style for the EditorGUILabel itself</param>
        public static void DrawDiffLabel(Rect rect, RenameResult renameResult, bool showBefore, DiffLabelStyle resultLabelStyle, GUIStyle style)
        {
            var labelText = string.Empty;
            if (!resultLabelStyle.HideDiff)
            {
                ApplyBackgroundColorToDiff(
                    rect,
                    style,
                    renameResult,
                    resultLabelStyle.OperationToShow,
                    resultLabelStyle.DiffBackgroundColor);
            }

            labelText = showBefore ? renameResult.GetOriginalColored(resultLabelStyle.DiffTextColor) :
                renameResult.GetResultColored(resultLabelStyle.DiffTextColor);
            EditorGUI.LabelField(rect, labelText, style);
        }

        private static void ApplyBackgroundColorToDiff(
            Rect rect,
            GUIStyle style,
            RenameResult renameContent,
            DiffOperation operationToColor,
            Color backgroundColor)
        {
            if (string.IsNullOrEmpty(renameContent.Original))
            {
                return;
            }

            // Blocks don't need padding or margin because it's accounted for
            // when we measure the total. We only want to know the size of each content block .
            var blockStyle = new GUIStyle(style);
            blockStyle.margin = new RectOffset();
            blockStyle.padding = new RectOffset();

            var position = rect;
            var allTextSoFar = string.Empty;
            foreach (var diff in renameContent)
            {
                // We want to skip whatever diff we aren't rendering on this column
                // (Column 1 shows deletions, Column 2 shows insertions)
                if (diff.Operation != operationToColor && diff.Operation != DiffOperation.Equal)
                {
                    continue;
                }

                if (diff.Operation == operationToColor)
                {
                    var totalRect = style.CalcSize(new GUIContent(allTextSoFar));
                    var blockRect = new Rect(position.x + totalRect.x - style.padding.left, position.y, 0, totalRect.y);
                    var spaceBlocks = GetConsecutiveBlocksOfToken(diff.Result, ' ');

                    foreach (var block in spaceBlocks)
                    {
                        var blockSize = blockStyle.CalcSize(new GUIContent(block));

                        blockRect.width = blockSize.x;
                        var textureWidth = ((blockRect.x + blockRect.width) < (rect.x + rect.width))
                            ? blockRect.width
                            : Mathf.Max(0f, (rect.x + rect.width) - (blockRect.x));

                        var textureRect = new Rect(
                            blockRect.x,
                            blockRect.y,
                            textureWidth,
                            blockRect.height);

                        var textColorTransparent = backgroundColor;

                        var oldColor = GUI.color;
                        GUI.color = textColorTransparent;
                        GUI.DrawTexture(textureRect, Texture2D.whiteTexture);
                        GUI.color = oldColor;

                        blockRect.x += blockSize.x;
                    }
                }

                allTextSoFar += diff.Result;
            }
        }

        private static List<string> GetConsecutiveBlocksOfToken(string str, char token)
        {
            var spaceBlocks = new List<string>();
            var characterStreak = new System.Text.StringBuilder();
            var isTokenBlock = false;
            if (str.Length > 0)
            {
                characterStreak.Append(str[0]);
                isTokenBlock = str[0] == token;
            }

            for (int i = 1; i < str.Length; ++i)
            {
                if (isTokenBlock)
                {
                    if (str[i] == token)
                    {
                        characterStreak.Append(str[i]);
                    }
                    else
                    {
                        spaceBlocks.Add(characterStreak.ToString());
                        characterStreak = new System.Text.StringBuilder();
                        characterStreak.Append(str[i]);

                        isTokenBlock = false;
                    }
                }
                else
                {
                    if (str[i] == token)
                    {
                        spaceBlocks.Add(characterStreak.ToString());
                        characterStreak = new System.Text.StringBuilder();
                        characterStreak.Append(str[i]);

                        isTokenBlock = true;
                    }
                    else
                    {
                        characterStreak.Append(str[i]);
                    }
                }
            }

            if (characterStreak.Length > 0)
            {
                spaceBlocks.Add(characterStreak.ToString());
            }

            return spaceBlocks;
        }

        public class DiffLabelStyle
        {
            public bool HideDiff { get; set; }

            public DiffOperation OperationToShow { get; set; }

            public Color DiffTextColor { get; set; }

            public Color DiffBackgroundColor { get; set; }
        }
    }
}