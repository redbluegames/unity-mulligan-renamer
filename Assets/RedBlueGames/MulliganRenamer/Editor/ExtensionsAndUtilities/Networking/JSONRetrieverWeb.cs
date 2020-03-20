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

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace RedBlueGames.MulliganRenamer
{
    public class JSONRetrieverWeb<T> : IJSONRetriever<T>
    {
        private IWebRequest requester;

        private AsyncOp<T> outstandingOp;

        public JSONRetrieverWeb(string url) : this(UnityWebRequestWrapper.Get(url))
        {
        }

        public JSONRetrieverWeb(IWebRequest requester)
        {
            this.requester = requester;
        }

        public AsyncOp<T> GetJSON()
        {
            this.outstandingOp = new AsyncOp<T>();
            EditorCoroutineUtility.StartBackgroundTask(this.Post(requester));
            return this.outstandingOp;
        }

        private IEnumerator Post(IWebRequest requester)
        {
            using (this.requester)
            {
                var startTime = Time.realtimeSinceStartup;
                requester.Timeout = 2;

                requester.SendWebRequest();
                while (!requester.IsDone)
                {
                    if (Time.realtimeSinceStartup - startTime > requester.Timeout)
                    {
                        this.outstandingOp.Status = AsyncStatus.Failed;
                        yield break;
                    }

                    if (requester.IsNetworkError)
                    {
                        this.outstandingOp.Status = AsyncStatus.Failed;
                        yield break;
                    }

                    yield return null;
                }

                this.outstandingOp.Status = AsyncStatus.Success;

                var json = JsonUtility.FromJson<T>(requester.DownloadedText);
                this.outstandingOp.ResultData = json;

                System.IO.File.WriteAllText("test.txt", requester.DownloadedText);
                Debug.Log(requester.DownloadedText);
            }
        }
    }
}