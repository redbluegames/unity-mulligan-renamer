/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

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

// The following #define can be uncommented to force a minimal delay forcing AsyncOps
// to return "Pending". This is useful for testing in-editor where most async ops are
// actually synchronous/instant.

// #define DELAY_ASYNCOP

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// AsyncOperation provides a way to query when an asynchronous operation has completed
    /// and what the result was.
    /// </summary>
    public class AsyncOp
    {
        /* Consts, Fields ========================================================================================================= */

        private AsyncStatus status;

#if DELAY_ASYNCOP
    private int pendingDelayCounter;
#endif

        /* Constructors, Enums ==================================================================================================== */

        /// <summary>
        /// Constructs a pending asynchronous operation.
        /// </summary>
        public AsyncOp()
        {
            this.Status = AsyncStatus.Pending;
        }

        /// <summary>
        /// Constructs the asynchronous operations. Status can be set directly if the
        /// operation is synchronous on a certain platform.
        /// </summary>
        public AsyncOp(AsyncStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        /// The current status of the operation.
        /// </summary>
        public AsyncStatus Status
        {
#if DELAY_ASYNCOP
            get 
            {
                return (++this.pendingDelayCounter < 250) ? AsyncStatus.Pending : this.status;
            }
#else
            get
            {
                return this.status;
            }
#endif
            set
            {
                this.status = value;
            }
        }

        public string FailureCode { get; set; }

        public string FailureMessage { get; set; }

        public IEnumerator WaitForResult(float timeout, System.Action timeoutCallback)
        {
            var startTime = Time.realtimeSinceStartup;
            while (this.Status == AsyncStatus.Pending)
            {
                if (Time.realtimeSinceStartup - startTime > timeout)
                {
                    if (timeoutCallback != null)
                    {
                        timeoutCallback.Invoke();
                    }

                    break;
                }

                yield return null;
            }
        }
    }

    /// <summary>
    /// A derived <see cref="AsyncOp"/> that allows for extra result information to be provided to the caller.
    /// </summary>
    /// <typeparam name="T">The type of result information to provide.</typeparam>
    public class AsyncOp<T> : AsyncOp
    {
        public AsyncOp() : base() { }

        public AsyncOp(AsyncStatus status) : base(status) { }

        public T ResultData { get; set; }
    }
}