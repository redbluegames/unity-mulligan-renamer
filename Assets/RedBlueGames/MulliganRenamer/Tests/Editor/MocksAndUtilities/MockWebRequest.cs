namespace RedBlueGames.MulliganRenamer
{
    using System;
    using UnityEngine.Networking;

    public class MockWebRequest : IWebRequest, IDisposable
    {
        private string mockDownloadText;

        public int Timeout { get; set; }

        public bool IsDone { get; private set; }

        public bool IsNetworkError
        {
            get
            {
                return false;
            }
        }

        public bool IsTimeout
        {
            get
            {
                return false;
            }
        }

        public string DownloadedText { get; private set; }

        public MockWebRequest(string outputText)
        {
            this.mockDownloadText = outputText;

            // Must SendWebRequest before we should consider it done.
            this.IsDone = false;
        }

        public void SendWebRequest()
        {
            this.DownloadedText = this.mockDownloadText;
            this.IsDone = true;
        }

        public void Dispose()
        {
        }
    }
}