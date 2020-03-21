namespace RedBlueGames.MulliganRenamer
{
    using System;
    using UnityEngine.Networking;

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

        public static UnityWebRequestWrapper Get(string url)
        {
            return new UnityWebRequestWrapper(url);
        }

        private UnityWebRequestWrapper(string url)
        {
            this.webRequest = UnityWebRequest.Get(url);
        }

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