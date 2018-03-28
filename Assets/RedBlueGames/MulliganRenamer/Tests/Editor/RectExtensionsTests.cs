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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NUnit.Framework;

    public class RectExtensionsTests
    {
        [Test]
        public void SplitVerticalWeighted_SingleSlice_NoChange()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var sameRect = new Rect(rect);

            // Act
            var result = rect.GetSplitVerticalWeighted(1, 0.0f, new float[] { 1.0f });

            // Assert
            Assert.AreEqual(sameRect, result);
        }

        [Test]
        public void SplitVerticalWeighted_TwoEvenSlices_SlicedEvenly()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstHalfRect = new Rect(Vector2.zero, new Vector2(100,50));
            var secondHalfRect = new Rect(new Vector2(0, 50), new Vector2(100, 50));

            // Act
            var firstSlice = rect.GetSplitVerticalWeighted(1, 0.0f, new float[] { 1.0f, 1.0f });
            var secondSlice = rect.GetSplitVerticalWeighted(2, 0.0f, new float[] { 1.0f, 1.0f });

            // Assert
            Assert.AreEqual(firstHalfRect, firstSlice);
            Assert.AreEqual(secondHalfRect, secondSlice);
        }

        [Test]
        public void SplitVerticalWeighted_TwoWeightedSlices_ProperlyWeighted()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstQuarterRect = new Rect(Vector2.zero, new Vector2(100, 25));
            var threeQuartersRect = new Rect(new Vector2(0, 25), new Vector2(100, 75));

            // Act
            var firstSlice = rect.GetSplitVerticalWeighted(1, 0.0f, new float[] { 1.0f, 3.0f });
            var secondSlice = rect.GetSplitVerticalWeighted(2, 0.0f, new float[] { 1.0f, 3.0f });

            // Assert
            Assert.AreEqual(firstQuarterRect, firstSlice);
            Assert.AreEqual(threeQuartersRect, secondSlice);
        }

        [Test]
        public void SplitVerticalWeighted_WeightedEvenlyWithSpace_ProperlyWeighted()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstQuarterRect = new Rect(Vector2.zero, new Vector2(100, 24));
            var secondQuarterRect = new Rect(new Vector2(0, 25), new Vector2(100, 24));
            var thirdQuarterRect = new Rect(new Vector2(0, 50), new Vector2(100, 24));
            var fourthQuarterRect = new Rect(new Vector2(0, 75), new Vector2(100, 24));

            // Act
            var spacing = 1.0f;
            var weights = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            var firstSlice = rect.GetSplitVerticalWeighted(1, spacing, weights);
            var secondSlice = rect.GetSplitVerticalWeighted(2, spacing, weights);
            var thirdSlice = rect.GetSplitVerticalWeighted(3, spacing, weights);
            var fourthSlice = rect.GetSplitVerticalWeighted(4, spacing, weights);

            // Assert
            Assert.AreEqual(firstQuarterRect, firstSlice);
            Assert.AreEqual(secondQuarterRect, secondSlice);
            Assert.AreEqual(thirdQuarterRect, thirdSlice);
            Assert.AreEqual(fourthQuarterRect, fourthSlice);
        }

        [Test]
        public void SplitHorizontalWeighted_SingleSlice_NoChange()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var sameRect = new Rect(rect);

            // Act
            var result = rect.GetSplitHorizontalWeighted(1, 0.0f, new float[] { 1.0f });

            // Assert
            Assert.AreEqual(sameRect, result);
        }

        [Test]
        public void SplitHorizontalWeighted_TwoEvenSlices_SlicedEvenly()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstHalfRect = new Rect(Vector2.zero, new Vector2(50, 100));
            var secondHalfRect = new Rect(new Vector2(50, 0), new Vector2(50, 100));

            // Act
            var firstSlice = rect.GetSplitHorizontalWeighted(1, 0.0f, new float[] { 1.0f, 1.0f });
            var secondSlice = rect.GetSplitHorizontalWeighted(2, 0.0f, new float[] { 1.0f, 1.0f });

            // Assert
            Assert.AreEqual(firstHalfRect, firstSlice);
            Assert.AreEqual(secondHalfRect, secondSlice);
        }

        [Test]
        public void SplitHorizontalWeighted_TwoWeightedSlices_ProperlyWeighted()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstQuarterRect = new Rect(Vector2.zero, new Vector2(25, 100));
            var threeQuartersRect = new Rect(new Vector2(25, 0), new Vector2(75, 100));

            // Act
            var firstSlice = rect.GetSplitHorizontalWeighted(1, 0.0f, new float[] { 1.0f, 3.0f });
            var secondSlice = rect.GetSplitHorizontalWeighted(2, 0.0f, new float[] { 1.0f, 3.0f });

            // Assert
            Assert.AreEqual(firstQuarterRect, firstSlice);
            Assert.AreEqual(threeQuartersRect, secondSlice);
        }

        [Test]
        public void SplitHorizontalWeighted_WeightedEvenlyWithSpace_ProperlyWeighted()
        {
            // Arrange
            Rect rect = new Rect(Vector2.zero, new Vector2(100, 100));

            var firstQuarterRect = new Rect(Vector2.zero, new Vector2(24, 100));
            var secondQuarterRect = new Rect(new Vector2(25, 0), new Vector2(24, 100));
            var thirdQuarterRect = new Rect(new Vector2(50, 0), new Vector2(24, 100));
            var fourthQuarterRect = new Rect(new Vector2(75, 0), new Vector2(24, 100));

            // Act
            var spacing = 1.0f;
            var weights = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            var firstSlice = rect.GetSplitHorizontalWeighted(1, spacing, weights);
            var secondSlice = rect.GetSplitHorizontalWeighted(2, spacing, weights);
            var thirdSlice = rect.GetSplitHorizontalWeighted(3, spacing, weights);
            var fourthSlice = rect.GetSplitHorizontalWeighted(4, spacing, weights);

            // Assert
            Assert.AreEqual(firstQuarterRect, firstSlice);
            Assert.AreEqual(secondQuarterRect, secondSlice);
            Assert.AreEqual(thirdQuarterRect, thirdSlice);
            Assert.AreEqual(fourthQuarterRect, fourthSlice);
        }
    }
}