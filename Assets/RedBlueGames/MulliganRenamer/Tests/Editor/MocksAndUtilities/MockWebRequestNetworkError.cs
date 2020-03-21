namespace RedBlueGames.MulliganRenamer
{
    using System;

    public class MockWebRequestNetworkError : IWebRequest, IDisposable
    {
        public int Timeout { get; set; }

        public bool IsDone
        {
            get
            {
                return false;
            }
        }

        public bool IsNetworkError { get; private set; }

        public bool IsTimeout
        {
            get
            {
                return false;
            }
        }

        public string DownloadedText
        {
            get
            {
                return string.Empty;
            }
        }

        public MockWebRequestNetworkError()
        {
            this.IsNetworkError = false;
        }

        public void SendWebRequest()
        {
            this.IsNetworkError = true;
        }

        public void Dispose()
        {
        }
    }
}