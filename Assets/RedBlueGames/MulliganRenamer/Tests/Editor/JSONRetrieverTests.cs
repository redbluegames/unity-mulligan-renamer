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
    using UnityEngine.TestTools;
    using NUnit.Framework;

    public class JSONRetrieverTests
    {
        [UnityTest]
        public IEnumerator GetJSON_ValidURLValidJSON_ReturnsExpected()
        {
            // Assemble
            var simpleJson = new SimpleJson();
            simpleJson.AnInt = 5;

            // Act
            var getter = new JSONRetrieverWeb<SimpleJson>("https://raw.githubusercontent.com/redbluegames/unity-mulligan-renamer/languages-from-web/Assets/RedBlueGames/MulliganRenamer/Tests/Editor/SimpleJson.json");
            var op = getter.GetJSON();

            var startTime = Time.realtimeSinceStartup;
            var timeout = 3.0f;
            while (op.Status.Equals(AsyncStatus.Pending))
            {
                if (Time.realtimeSinceStartup - startTime > timeout)
                {
                    Assert.Fail("Test timed out. AsyncOp never returned a Status besides Pending");
                    yield break;
                }

                yield return null;
            }

            Assert.AreEqual(simpleJson.AnInt, op.ResultData.AnInt);
        }

        [Test]
        public void GetJSON_InvalidURL_ThrowsBadURL()
        {
            Assert.Fail();
        }

        [Test]
        public void GetJSON_InvalidJSON_ThrowsBadJSON()
        {
            Assert.Fail();
        }

        [Test]
        public void GetJSON_InvalidUrl_Timeout()
        {
            Assert.Fail();
        }

        [System.Serializable]
        public class SimpleJson
        {
            [SerializeField]
            private int anInt;

            public int AnInt
            {
                get
                {
                    return this.anInt;
                }

                set
                {
                    this.anInt = value;
                }
            }
        }
    }
}