namespace RedBlueGames.MulliganRenamer
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class UnityWebRequestFacade : IWebRequest
    {
        private UnityWebRequest webRequest;

        public bool IsDone
        {
            get
            {
                return this.webRequest.isDone;
            }
        }

        public static UnityWebRequestFacade Get(string url)
        {
            return new UnityWebRequestFacade(url);
        }

        private UnityWebRequestFacade(string url)
        {
            this.webRequest = UnityWebRequest.Get(url);
        }

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return this.webRequest.SendWebRequest();
        }

        public void Dispose()
        {

        }
    }
}