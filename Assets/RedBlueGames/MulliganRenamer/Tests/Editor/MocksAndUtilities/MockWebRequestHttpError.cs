namespace RedBlueGames.MulliganRenamer
{
    using System;

    public class MockWebRequestHttpError : IWebRequest, IDisposable
    {
        public int Timeout { get; set; }

        public bool IsDone { get; private set; }

        public bool IsNetworkError
        {
            get
            {
                return false;
            }
        }

        public bool IsHttpError { get; private set; }

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
                return "Http error message.";
            }
        }

        public string DownloadedText
        {
            get
            {
                return string.Empty;
            }
        }

        public MockWebRequestHttpError()
        {
            this.IsHttpError = false;
        }

        public void SendWebRequest()
        {
            this.IsDone = true;
            this.IsHttpError = true;
        }

        public void Dispose()
        {
        }
    }
}