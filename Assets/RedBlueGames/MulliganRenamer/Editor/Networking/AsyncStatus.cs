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

namespace RedBlueGames.MulliganRenamer
{
    /// <summary>
    /// The status of an asynchronous operation. Custom AsyncStatus singleton instances
    /// can be created to denote specific types of failure. A generic Failure status is
    /// also included below but note that it may not be the only type of failure assigned.
    /// </summary>
    public sealed class AsyncStatus
    {
        public AsyncStatus(string description) { this.Description = description; }

        /// <summary>
        /// The description of this status.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Status result for async operations that have not completed.
        /// </summary>
        public static readonly AsyncStatus Pending = new AsyncStatus("Pending");

        /// <summary>
        /// Status result for successfully completed async operations.
        /// </summary>
        public static readonly AsyncStatus Success = new AsyncStatus("Success");

        /// <summary>
        /// Status result for canceled async operations.
        /// </summary>
        public static readonly AsyncStatus Canceled = new AsyncStatus("Canceled");

        /// <summary>
        /// Status result for a timedout async operations.
        /// </summary>
        public static readonly AsyncStatus Timeout = new AsyncStatus("Timeout");

        /// <summary>
        /// Status result for a failed async operations. Note that other custom failure
        /// types may be created, so *avoid* (op == AsyncOp.Failed) style checks.
        /// </summary>
        public static readonly AsyncStatus Failed = new AsyncStatus("Failed");

        public override string ToString()
        {
            return this.Description;
        }
    }
}