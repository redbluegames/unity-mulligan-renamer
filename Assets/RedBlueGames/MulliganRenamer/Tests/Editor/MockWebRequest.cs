namespace RedBlueGames.MulliganRenamer
{
    using System;
    using UnityEngine.Networking;

    public class MockWebRequest : IWebRequest, IDisposable
    {
        public int Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                return true;
            }
        }

        public bool IsNetworkError
        {
            get
            {
                return false;
            }
        }

        public string DownloadedText { get; private set; }

        public MockWebRequest(string outputText)
        {
            this.DownloadedText = outputText;
        }

        public void SendWebRequest()
        {
        }

        public void Dispose()
        {
        }
    }
}