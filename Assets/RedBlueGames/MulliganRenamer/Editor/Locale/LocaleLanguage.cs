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
    public class LocaleLanguage : System.IEquatable<LocaleLanguage>
    {
        // These are assigned through Unity's serialization / deserialization.
        // So we Ignore warning about Unassigned field.
#pragma warning disable 0649
        [SerializeField]
        private string languageName;
        [SerializeField]
        private string languageKey;
        [SerializeField]
        private List<Locale> elements;
#pragma warning restore 0649

        public string LanguageName
        {
            get
            {
                return languageName;
            }
        }

        public string LanguageKey
        {
            get
            {
                return languageKey;
            }
        }

        public List<Locale> Elements
        {
            get
            {
                return elements;
            }
        }

        public bool IsActive
        {
            get
            {
                return LocaleManager.Instance.CurrentLanguage == this;
            }
        }

        public string GetValue(string key)
        {
            var locale = elements.Find(x => x.Key.Equals(key));
            return locale != null ? locale.Value : "A0";
        }

        /// <summary>
        /// Compares two <see cref="LocaleLanguage"/> objects for inequality.
        /// </summary>
        /// <param name="languageA">One of the <see cref="LocaleLanguage"/> objects to compare.</param>
        /// <param name="languageB">The other <see cref="LocaleLanguage"/> object to compare.</param>
        /// <returns>Returns true if the two <see cref="LocaleLanguage"/> objects are not equal - otherwise, false.</returns>
        public static bool operator ==(LocaleLanguage languageA, LocaleLanguage languageB)
        {
            return languageA.Equals(languageB);
        }

        /// <summary>
        /// Compares two <see cref="LocaleLanguage"/> objects for inequality.
        /// </summary>
        /// <param name="languageA">One of the <see cref="LocaleLanguage"/> objects to compare.</param>
        /// <param name="languageB">The other <see cref="LocaleLanguage"/> object to compare.</param>
        /// <returns>Returns true if the two <see cref="LocaleLanguage"/> objects are not equal - otherwise, false.</returns>
        public static bool operator !=(LocaleLanguage languageA, LocaleLanguage languageB)
        {
            return !languageA.Equals(languageB);
        }

        /// <summary>
        /// Compares this <see cref="LocaleLanguage"/> to the supplied object for equality.
        /// </summary>
        /// <param name="obj">The other object to compare this <see cref="LocaleLanguage"/> against.</param>
        /// <returns>Returns true if the two <see cref="LocaleLanguage"/> objects are equal - otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is LocaleLanguage)
            {
                return this.Equals((LocaleLanguage)obj);
            }

            return false;
        }

        /// <summary>
        /// Compares this <see cref="LocaleLanguage"/> to the supplied object for equality.
        /// </summary>
        /// <param name="obj">The other object to compare this <see cref="LocaleLanguage"/> against.</param>
        /// <returns>Returns true if the two <see cref="LocaleLanguage"/> objects are equal - otherwise, false.</returns>
        public bool Equals(LocaleLanguage other)
        {
            if (this.LanguageKey != other.LanguageKey)
            {
                return false;
            }

            if (this.languageName != other.LanguageName)
            {
                return false;
            }

            if (!Enumerable.SequenceEqual(this.Elements, other.Elements))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// /// Gets a hash code representation of this <see cref="LocaleLanguage"/>.
        /// </summary>
        /// <returns>Returns a hash code representation of this <see cref="LocaleLanguage"/>.</returns>
        public override int GetHashCode()
        {
            return this.LanguageKey.GetHashCode();
        }

        /// <summary>
        /// Converts this <see cref="LocaleLanguage"/> to a string.
        /// </summary>
        /// <returns>A string representation of this <see cref="LocaleLanguage"/>.</returns>
        public override string ToString()
        {
            return this.LanguageName;
        }
    }
}