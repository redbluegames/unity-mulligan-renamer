namespace RedBlueGames.MulliganRenamer
{
    using System;

    public class MockWebRequestNetworkError : IWebRequest, IDisposable
    {
        public int Timeout { get; set; }

        public bool IsDone { get; private set; }

        public bool IsNetworkError { get; private set; }

        public bool IsHttpError
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

        public string ErrorText
        {
            get
            {
                return "Network error message.";
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
            this.IsDone = true;
            this.IsNetworkError = true;
        }

        public void Dispose()
        {
        }
    }
}