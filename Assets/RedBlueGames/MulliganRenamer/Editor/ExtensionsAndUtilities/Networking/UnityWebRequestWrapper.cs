/* MIT License

Copyright (c) 2020 Edward Rowe

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
    using System;
    using UnityEngine.Networking;

    /// <summary>
    /// This Wrapper is used so that we can better test JSONRetrievers by coding
    /// to a common interface. The wrapper exposes only the functionality we need
    /// for JSONRetrievers, which is defined in IWebRequest.
    /// </summary>
    public class UnityWebRequestWrapper : IWebRequest, IDisposable
    {
        private UnityWebRequest webRequest;

        public int Timeout
        {
            get
            {
                return this.webRequest.timeout;
            }

            set
            {
                this.webRequest.timeout = value;
            }
        }

        public bool IsTimeout
        {
            get
            {
                return this.webRequest.isNetworkError && this.webRequest.error == "Request timeout";
            }
        }

        public bool IsDone
        {
            get
            {
                return this.webRequest.isDone;
            }
        }

        public bool IsNetworkError
        {
            get
            {
                return this.webRequest.isNetworkError;
            }
        }

        public string DownloadedText
        {
            get
            {
                return this.webRequest.downloadHandler.text;
            }
        }

        /// <summary>
        /// Create an instance ready to go for a GET request. This mirror's UnityWebRequest's API.
        /// </summary>
        /// <param name="uri">Address for the GET</param>
        /// <returns>The object that can be used to execute the web request</returns>
        public static UnityWebRequestWrapper Get(string uri)
        {
            return new UnityWebRequestWrapper(uri);
        }

        private UnityWebRequestWrapper(string uri)
        {
            this.webRequest = UnityWebRequest.Get(uri);
        }

        /// <summary>
        /// Asynchronously send the request to the web. Query IsDone to see if it's complete.
        /// </summary>
        public void SendWebRequest()
        {
            this.webRequest.SendWebRequest();
        }

        public void Dispose()
        {
            this.webRequest.Dispose();
        }
    }
}