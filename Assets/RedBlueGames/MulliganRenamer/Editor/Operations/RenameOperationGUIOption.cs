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
    /// GUI options to apply when drawing a RenameOperation
    /// </summary>
    public class RenameOperationGUIOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenameOperation"/>
        /// should be drawn with the Up Button disabled.
        /// </summary>
        /// <value><c>true</c> if the up button should be disabled; otherwise, <c>false</c>.</value>
        public bool DisableUpButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenameOperation"/>
        /// should be drawn with the Down Button disabled.
        /// </summary>
        /// <value><c>true</c> if the down button should be disabled; otherwise, <c>false</c>.</value>
        public bool DisableDownButton { get; set; }

        /// <summary>
        /// Gets or sets the prefix to use when setting control names for this <see cref="RenameOperation"/>
        /// </summary>
        /// <value>The control prefix.</value>
        public int ControlPrefix { get; set; }
    }
}