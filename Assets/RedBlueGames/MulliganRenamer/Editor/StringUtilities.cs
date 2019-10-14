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
    using System.Text.RegularExpressions;

    public static class StringUtilities
    {
        public static string[] StripCommasFromString(string stringWithComas)
        {
            var splits = stringWithComas.Split(',');
            return splits;
        }

        public static string AddCommasBetweenStrings(string[] strings)
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

        const string HTML_TAG_PATTERN = "<.*?>";
        public static string StripHTML(string inputString, string replaceWith = "")
        {
            return Regex.Replace(inputString, HTML_TAG_PATTERN, replaceWith);
        }
    }
}