/* MIT License

Copyright (c) 2019 Murillo Pugliesi Lopes, https://github.com/Mukarillo

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
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Class responsible for holding all the translated elements
    /// you can get a translated content by its key calling Get method
    /// </summary>
	[System.Serializable]
    public class Language
    {
        // These are assigned through Unity's serialization / deserialization.
        // So we Ignore warning about Unassigned field.
#pragma warning disable 0649
        [SerializeField]
        private string name;

        [SerializeField]
        private string key;

        [SerializeField]
        private int version;

        [SerializeField]
        private List<LocalizedString> elements;
#pragma warning restore 0649

        /// <summary>
        /// The Version of the language, used to determine whether or not data has been updated.
        /// This should be incremented when changes are made.
        /// </summary>
        public int Version
        {
            get
            {
                return this.version;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Key
        {
            get
            {
                return key;
            }
        }

        public List<LocalizedString> Elements
        {
            get
            {
                return elements;
            }
        }

        public string GetValue(string key)
        {
            var language = elements.Find(x => x.Key.Equals(key));
            return language != null ? language.Value : "A0";
        }
    }
}