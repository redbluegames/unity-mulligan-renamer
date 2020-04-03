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
    /// <summary>
    /// GUI control name utility provides functions that help give unique names to controls that
    /// might have duplicates.
    /// </summary>
    public static class GUIControlNameUtility
    {
        private static readonly char Delimeter = '|';

        /// <summary>
        /// Gets the prefix of the control from a name that's been created with this utility.
        /// Names are of the format "#|Control" where # is the prefix value.
        /// </summary>
        /// <returns>The prefix in the name.</returns>
        /// <param name="name">Name that has a prefix.</param>
        public static int GetPrefixFromName(string name)
        {
            var split = name.Split(Delimeter);
            int prefix = -1;

            if (split.Length != 2)
            {
                var exception = string.Format(
                                    "Expected name of format '#|ControlName' but it did not parse correctly. Argument: {0}", name);
                throw new System.ArgumentException(exception);
            }

            int.TryParse(split[0], out prefix);
            return prefix;
        }

        /// <summary>
        /// Returns a value indicating whether or not the supplied string was created with this Prefixing class.
        /// The expected format is #|Field
        /// </summary>
        /// <param name="controlName">Field name to parse for prefix format</param>
        /// <returns>true if the control name is prefixed, false otherwise</returns>
        public static bool IsControlNamePrefixed(string controlName)
        {
            var split = controlName.Split(Delimeter);
            if (split.Length != 2)
            {
                return false;
            }

            int prefixValue = -1;
            int.TryParse(split[0], out prefixValue);

            return prefixValue >= 0;
        }

        /// <summary>
        /// Creates a prefixed name from a prefix and control name.
        /// Names are of the format "#|Control" where # is the prefix value.
        /// </summary>
        /// <returns>The prefixed name.</returns>
        /// <param name="prefix">Prefix to concatenate.</param>
        /// <param name="controlName">Control name.</param>
        public static string CreatePrefixedName(int prefix, string controlName)
        {
            return string.Concat(prefix.ToString(), Delimeter, controlName);
        }
    }
}