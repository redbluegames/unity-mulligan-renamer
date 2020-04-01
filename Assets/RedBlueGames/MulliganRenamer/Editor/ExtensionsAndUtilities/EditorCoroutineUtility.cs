namespace RedBlueGames.MulliganRenamer
{
    using System;
    using System.Collections;
    using UnityEditor;
    using UnityEngine;

    public static class EditorCoroutineUtility
    {
        /// <summary>
        /// Starts a Coroutine using Unity Editor's update loop. This is useful for EditorWindows
        /// which aren't MonoBehaviours and therefore can't use Coroutines.
        /// Utility function adapted from https://forum.unity.com/threads/using-unitywebrequest-in-editor-tools.397466/#post-4485181
        /// </summary>
        /// <param name="update"></param>
        /// <param name="onEnd"></param>
        public static void StartBackgroundTask(IEnumerator update, Action onEnd = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        if (onEnd != null)
                        {
                            onEnd();
                        }

                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    if (onEnd != null)
                    {
                        onEnd();
                    }

                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}