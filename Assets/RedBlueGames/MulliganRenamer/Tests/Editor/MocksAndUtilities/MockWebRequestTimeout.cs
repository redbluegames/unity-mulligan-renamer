namespace RedBlueGames.MulliganRenamer
{
    using System;

    public class MockWebRequestTimeout : IWebRequest, IDisposable
    {
        public int Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                return false;
            }
        }

        public bool IsNetworkError
        {
            get
            {
                return true;
            }
        }

        public bool IsTimeout
        {
            get
            {
                return true;
            }
        }

        public string DownloadedText
        {
            get
            {
                return string.Empty;
            }
            private set
            {
            }
        }

        public MockWebRequestTimeout()
        {
        }

        public void SendWebRequest()
        {
        }

        public void Dispose()
        {
        }
    }
}