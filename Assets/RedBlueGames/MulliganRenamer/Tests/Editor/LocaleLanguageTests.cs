/* MIT License

Copyright (c) 2020 Murillo Pugliesi Lopes, https://github.com/Mukarillo

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
    using NUnit.Framework;

    public class LocaleLanguageTests
    {
        private List<LocaleLanguage> allResourceLanguages;

        [SetUp]
        public void Init()
        {
            this.allResourceLanguages = LocaleManager.LoadAllLanguages();
        }

        [Test]
        public void CheckAllLanguageKeys_WithEnglish()
        {
            var englishLanguage = this.allResourceLanguages.Find(x => x.LanguageKey == "en");

            foreach (var language in this.allResourceLanguages)
            {
                if (language.LanguageKey == "en")
                    continue;

                foreach (var englishElement in englishLanguage.Elements)
                {
                    Assert.That(
                        language.Elements.Exists(x => x.Key == englishElement.Key),
                        "Language " + language.LanguageName + " does not contain key \"" + englishElement.Key +
                        "\". Please add an element with this key to the language.");
                }
            }
        }

        [Test]
        public void CheckAllLanguages_HasValue()
        {
            foreach (var language in this.allResourceLanguages)
            {
                foreach (var element in language.Elements)
                {
                    Assert.That(!string.IsNullOrEmpty(element.Value),
                        "Language " + language.LanguageName + " does not contain a value for key \"" + element.Key +
                        "\". Please add a value to the language.");
                }
            }
        }
    }
}