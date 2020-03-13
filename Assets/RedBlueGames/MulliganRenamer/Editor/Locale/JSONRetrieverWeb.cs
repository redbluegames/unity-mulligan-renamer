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

        private AsyncOp<T> outstandingOp;

        public AsyncOp<T> GetJSON(string url)
        {
            this.outstandingOp = new AsyncOp<T>();
            EditorCoroutineUtility.StartBackgroundTask(this.Post(url));
            return this.outstandingOp;
        }

        private IEnumerator Post(string uri)
        {
            var startTime = Time.realtimeSinceStartup;
            using (UnityWebRequest w = UnityWebRequest.Get(uri))
            {
                w.timeout = 2;

                var webOp = w.SendWebRequest();

                while (!webOp.isDone)
                {
                    if (Time.realtimeSinceStartup - startTime > w.timeout)
                    {
                        this.outstandingOp.Status = AsyncStatus.Failed;
                        yield break;
                    }

                    if (w.isNetworkError)
                    {
                        this.outstandingOp.Status = AsyncStatus.Failed;
                        yield break;
                    }

                    yield return null;
                }

                this.outstandingOp.Status = AsyncStatus.Success;

                var json = JsonUtility.FromJson<T>(w.downloadHandler.text);
                this.outstandingOp.ResultData = json;

                System.IO.File.WriteAllText("test.txt", w.downloadHandler.text);
                Debug.Log(w.downloadHandler.text);
            }
        }
    }
}