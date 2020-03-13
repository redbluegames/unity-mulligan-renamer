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
            var mockWebRequest = new MockWebRequest("{ \"anInt\": 5}");
            var getter = new JSONRetrieverWeb<SimpleJson>(mockWebRequest);
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Test timed out. AsyncOp never returned a Status besides Pending."));

            Assert.AreEqual(AsyncStatus.Success, op.Status);
            Assert.AreEqual(simpleJson.AnInt, op.ResultData.AnInt);
        }

        [Test]
        public void GetJSON_InvalidURI_ThrowsBadURI()
        {
            Assert.Throws<System.ArgumentException>(() => new JSONRetrieverWeb<SimpleJson>("InvalidUri"));
        }

        [UnityTest]
        public IEnumerator GetJSON_InvalidJSON_ThrowsBadJSON()
        {
            // Assemble
            var badJson = "{ \"anIntWithNoValue\" } ";

            // Act
            var mockWebRequest = new MockWebRequest(badJson);
            var getter = new JSONRetrieverWeb<SimpleJson>(mockWebRequest);
            //Assert.Throws<System.ArgumentException>(() => getter.GetJSON(1), "Expected ArgumentException due to invalid Json format.");
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Unexpected timeout. Test timed out, expected InvalidJSON exception"));

            Assert.AreEqual(AsyncStatus.Failed, op.Status);
            Assert.AreEqual(JSONRetrieverWeb<SimpleJson>.ErrorCodeInvalidJsonFormat, op.FailureCode);
        }

        [UnityTest]
        public IEnumerator GetJSON_Timeout_ReportsTimeout()
        {
            // Assemble
            // Act
            var mockTimeout = new MockWebRequestTimeout();
            var getter = new JSONRetrieverWeb<SimpleJson>(mockTimeout);
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Unexpected timeout. JsonRetrieverWeb should have sent a timeout, but did not."));

            Assert.AreEqual(AsyncStatus.Timeout, op.Status);
        }

        [UnityTest]
        public IEnumerator GetJSON_HttpError_ReportsFail()
        {
            // Assemble
            // Act
            var mockHttpError = new MockWebRequestHttpError();
            var getter = new JSONRetrieverWeb<SimpleJson>(mockHttpError);
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Unexpected timeout. JsonRetrieverWeb should have sent a failure, but did not."));

            Assert.AreEqual(AsyncStatus.Failed, op.Status);
        }

        [UnityTest]
        public IEnumerator GetJSON_NetworkError_Fails()
        {
            // Assemble
            // Act
            var mockError = new MockWebRequestNetworkError();
            var getter = new JSONRetrieverWeb<SimpleJson>(mockError);
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Unexpected timeout. JsonRetrieverWeb should have sent a failure, but did not."));

            Assert.AreEqual(AsyncStatus.Failed, op.Status);
        }

        [UnityTest]
        public IEnumerator GetJSON_ReuseRetriever_ReturnsSameResultTwice()
        {
            // Assemble
            var simpleJson = new SimpleJson();
            simpleJson.AnInt = 3;

            // Act
            var mockWebRequest = new MockWebRequest("{ \"anInt\": 3}");
            var getter = new JSONRetrieverWeb<SimpleJson>(mockWebRequest);
            var op = getter.GetJSON(1);
            yield return op.WaitForResult(1.5f, () => Assert.Fail("Test timed out on first Get. AsyncOp never returned a Status besides Pending."));

            Assert.AreEqual(AsyncStatus.Success, op.Status);
            Assert.AreEqual(simpleJson.AnInt, op.ResultData.AnInt);

            var op2 = getter.GetJSON(1);
            yield return op2.WaitForResult(1.5f, () => Assert.Fail("Test timed out on second Get. AsyncOp never returned a Status besides Pending."));

            Assert.AreEqual(AsyncStatus.Success, op.Status);
            Assert.AreEqual(simpleJson.AnInt, op.ResultData.AnInt);
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