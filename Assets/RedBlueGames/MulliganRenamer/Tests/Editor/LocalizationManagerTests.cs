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
    using NUnit.Framework;
    using System;

    public class LocalizationManagerTests
    {
        private Language languageBeforeTest;

        [SetUp]
        public void Init()
        {
            this.languageBeforeTest = LocalizationManager.Instance.CurrentLanguage;
        }

        [TearDown]
        public void Cleanup()
        {
            LocalizationManager.Instance.ChangeLanguage(this.languageBeforeTest.Key);
        }

        [Test]
        public void ChangeLanguage_GoodKey_Changes()
        {
            foreach (var language in LocalizationManager.Instance.AllLanguages)
            {
                LocalizationManager.Instance.ChangeLanguage(language.Key);

                Assert.That(
                    language.Key.Equals(LocalizationManager.Instance.CurrentLanguage.Key),
                    "LocalizationManager did not change language to specified language, " + language.Name + ".");
            }
        }

        [Test]
        public void ChangeLanguage_BadKey_DoesNotChange()
        {
            var oldLanguage = LocalizationManager.Instance.CurrentLanguage;
            var receivedLanguageChangedEvent = false;
            LocalizationManager.Instance.LanguageChanged += () => receivedLanguageChangedEvent = true;
            LocalizationManager.Instance.ChangeLanguage("doesNotExist");
            Assert.AreEqual(oldLanguage, LocalizationManager.Instance.CurrentLanguage,
                "LocalizationManager changed language when passed an invalid key. It should remain unchanged.");
            Assert.False(receivedLanguageChangedEvent, "LocalizationManager fired off event for langauge changed" +
                ", but it should not have changed, or fired the event.");
        }
    }
}