namespace RedBlueGames.MulliganRenamer
{
    using UnityEngine;
    using UnityEditor;

    public static class RectExtensions
    {
        public static Rect AddPadding(this Rect rect, int left, int right, int top, int bottom)
        {
            // We want to think of padding as positive, not an offset, so invert it here to 
            // make the math work out.
            var offset = new RectOffset(-left, -right, -top, -bottom);
            return rect.AddOffset(offset);
        }

        public static Rect AddOffset(this Rect rect, RectOffset offsetAsPadding)
        {
            var paddedRect = offsetAsPadding.Add(rect);
            return paddedRect;
        }

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