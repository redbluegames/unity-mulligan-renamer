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