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
    using System;
    using UnityEngine;

    /// <summary>
    /// Get a JSON object from a specified address using the specified IWebRequest
    /// </summary>
    /// <typeparam name="T">Type of the JSON object to contstruct after fetching the file.</typeparam>
    public class JSONRetrieverWeb<T>
    {
        public static readonly string ErrorCodeNetworkError = "Network Error";
        public static readonly string ErrorCodeHttpError = "Http Error";
        public static readonly string ErrorCodeInvalidJsonFormat = "Invalid JSON format";

        private IWebRequest requester;

        private AsyncOp<T> outstandingOp;

        public JSONRetrieverWeb(string uri) : this(UnityWebRequestWrapper.Get(uri))
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                throw new System.ArgumentException("Invalid URI Format.");
            }
        }

        public JSONRetrieverWeb(IWebRequest requester)
        {
            this.requester = requester;
        }

        /// <summary>
        /// Request the JSON from the web using the initialized uri. 
        /// </summary>
        /// <param name="timeout">Timeout for the web request, which returns AsyncStatus of Timeout.</param>
        /// <returns>An AsyncOp. Query this for the status of the operation and for its results.</returns>
        public AsyncOp<T> GetJSON(int timeout)
        {
            this.outstandingOp = new AsyncOp<T>();
            EditorCoroutineUtility.StartBackgroundTask(this.Post(requester, timeout));
            return this.outstandingOp;
        }

        private IEnumerator Post(IWebRequest requester, int timeout)
        {
            using (this.requester)
            {
                requester.Timeout = timeout;

                requester.SendWebRequest();
                while (!requester.IsDone)
                {
                    yield return null;
                }

                if (requester.IsTimeout)
                {
                    this.outstandingOp.Status = AsyncStatus.Timeout;
                    yield break;
                }

                if (requester.IsNetworkError || requester.IsHttpError)
                {
                    this.outstandingOp.Status = AsyncStatus.Failed;
                    this.outstandingOp.FailureCode = requester.IsHttpError ? ErrorCodeHttpError : ErrorCodeNetworkError;
                    this.outstandingOp.FailureMessage = requester.ErrorText;
                    yield break;
                }

                this.outstandingOp.Status = AsyncStatus.Success;

                try
                {
                    var json = JsonUtility.FromJson<T>(requester.DownloadedText);
                    this.outstandingOp.ResultData = json;
                }
                catch (System.ArgumentException e)
                {
                    this.outstandingOp.Status = AsyncStatus.Failed;
                    this.outstandingOp.FailureCode = ErrorCodeInvalidJsonFormat;
                    this.outstandingOp.FailureMessage = e.Message;
                }
            }
        }
    }
}