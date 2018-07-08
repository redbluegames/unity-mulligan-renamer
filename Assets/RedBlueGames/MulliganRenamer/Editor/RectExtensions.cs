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
    using UnityEngine;
    using UnityEditor;

    public static class RectExtensions
    {
        /// <summary>
        /// Adds padding to the rect, where positive numbers shrink the sides.
        /// </summary>
        /// <returns>The padding.</returns>
        /// <param name="rect">Rect to modify.</param>
        /// <param name="left">Left padding.</param>
        /// <param name="right">Right padding.</param>
        /// <param name="top">Top padding.</param>
        /// <param name="bottom">Bottom padding.</param>
        public static Rect AddPadding(this Rect rect, int left, int right, int top, int bottom)
        {
            // We want to think of padding as positive, not an offset, so invert it here to 
            // make the math work out.
            var offset = new RectOffset(-left, -right, -top, -bottom);
            return rect.AddOffset(offset);
        }

        /// <summary>
        /// Adds the offset into the rect.
        /// </summary>
        /// <returns>The offset.</returns>
        /// <param name="rect">Rect to modify.</param>
        /// <param name="offsetAsPadding">Offset as padding.</param>
        public static Rect AddOffset(this Rect rect, RectOffset offsetAsPadding)
        {
            var paddedRect = offsetAsPadding.Add(rect);
            return paddedRect;
        }

        /// <summary>
        /// Splits the Rect horizontally into the specified number of divisions and returns the requested division.
        /// </summary>
        /// <returns>The split rect.</returns>
        /// <param name="rect">Rect to split.</param>
        /// <param name="division">Rect division to return.</param>
        /// <param name="numDivisions">Number of total divisions.</param>
        /// <param name="spacing">Spacing between divisions.</param>
        public static Rect GetSplitHorizontal(this Rect rect, int division, int numDivisions, float spacing)
        {
            if (numDivisions <= 0)
            {
                var message = "Tried to Split Rect Horizontal using illegal numDivisions. Divisions must be greater than 0";
                throw new System.ArgumentException(message, "numDivisions");
            }

            var divisionWeights = new float[numDivisions];
            for (int i = 0; i < divisionWeights.Length; ++i)
            {
                divisionWeights[i] = 1.0f;
            }

            return rect.GetSplitHorizontalWeighted(division, spacing, divisionWeights);
        }

        /// <summary>
        /// Splits the Rect horizontally into divisions, with each division getting a relative weight according to its
        /// weight in the weights array. Returns the requested division.
        /// </summary>
        /// <returns>The split horizontal weighted.</returns>
        /// <param name="rect">Rect to split.</param>
        /// <param name="division">Rect division to return.</param>
        /// <param name="spacing">Spacing between rects.</param>
        /// <param name="weights">Weights for each split.</param>
        public static Rect GetSplitHorizontalWeighted(this Rect rect, int division, float spacing, float[] weights)
        {
            if (weights == null || weights.Length <= 0)
            {
                var message = "Tried to Split Rect using illegal weights. " +
                    "At least one weight must be provided";
                throw new System.ArgumentException(message, "weights");
            }

            if (division <= 0 || division > weights.Length)
            {
                var message = string.Format("Tried to Split Rect using illegal division. " +
                                            "Division should be the desired split specified starting with 1. Divison: {0}", division);
                throw new System.ArgumentException(message, "division");
            }

            var split = new Rect(rect);
            var splitWidth = GetSplitSizeWeighted(division - 1, rect.width, spacing, weights);
            split.width = splitWidth;
            split.x = rect.x + GetSplitOffsetWeighted(division - 1, rect.width, spacing, weights);
            return split;
        }

        /// <summary>
        /// Splits the Rect vertically into the specified number of divisions and returns the requested division.
        /// </summary>
        /// <returns>The split rect.</returns>
        /// <param name="rect">Rect to split.</param>
        /// <param name="division">Rect division to return.</param>
        /// <param name="numDivisions">Number of total divisions.</param>
        /// <param name="spacing">Spacing between divisions.</param>
        public static Rect GetSplitVertical(this Rect rect, int division, int numDivisions, float spacing)
        {
            if (numDivisions <= 0)
            {
                var message = "Tried to Split Rect Vertical using illegal numDivisions. Divisions must be greater than 0";
                throw new System.ArgumentException(message, "numDivisions");
            }

            var divisionWeights = new float[numDivisions];
            for (int i = 0; i < divisionWeights.Length; ++i)
            {
                divisionWeights[i] = 1.0f;
            }

            return rect.GetSplitVerticalWeighted(division, spacing, divisionWeights);
        }

        /// <summary>
        /// Splits the Rect vertically into divisions, with each division getting a relative weight according to its
        /// weight in the weights array. Returns the requested division.
        /// </summary>
        /// <returns>The split horizontal weighted.</returns>
        /// <param name="rect">Rect to split.</param>
        /// <param name="division">Rect division to return.</param>
        /// <param name="spacing">Spacing between rects.</param>
        /// <param name="weights">Weights for each split.</param>
        public static Rect GetSplitVerticalWeighted(this Rect rect, int division, float spacing, float[] weights)
        {
            if (weights == null || weights.Length <= 0)
            {
                var message = "Tried to Split Rect using illegal weights. " +
                    "At least one weight must be provided";
                throw new System.ArgumentException(message, "weights");
            }

            if (division <= 0 || division > weights.Length)
            {
                var message = string.Format("Tried to Split Rect using illegal division. " +
                                            "Division should be the desired split specified starting with 1. Divison: {0}", division);
                throw new System.ArgumentException(message, "division");
            }

            var split = new Rect(rect);
            var splitHeight = GetSplitSizeWeighted(division - 1, rect.height, spacing, weights);
            split.height = splitHeight;
            split.y = rect.y + GetSplitOffsetWeighted(division - 1, rect.height, spacing, weights);
            return split;
        }

        private static float GetSplitSizeWeighted(int divisionIndex, float unsplitSize, float spacing, float[] weights)
        {
            var numDivisions = weights.Length;

            var weightTotal = 0.0f;
            for (int i = 0; i < weights.Length; ++i)
            {
                weightTotal += weights[i];
            }

            var sizeFromSpacing = (spacing * numDivisions);
            var sizeWithoutSpacing = unsplitSize - sizeFromSpacing;
            var normalizedWeight = weights[divisionIndex] / weightTotal;
            var splitHeight = sizeWithoutSpacing * normalizedWeight;
            return splitHeight;
        }

        private static float GetSplitOffsetWeighted(int divisionIndex, float unsplitSize, float spacing, float[] weights)
        {
            var heightFromPreviousSlices = 0.0f;
            for (int i = 0; i < divisionIndex; ++i)
            {
                heightFromPreviousSlices += GetSplitSizeWeighted(i, unsplitSize, spacing, weights);
                heightFromPreviousSlices += spacing;
            }

            return heightFromPreviousSlices;
        }
    }
}