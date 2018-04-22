namespace RedBlueGames.MulliganRenamer
{
    /// <summary>
    /// Events that are returned by the GUI draw call to indicate what input was pressed.
    /// </summary>
    public enum RenameOperationSortingButtonEvent
    {
        /// <summary>
        /// No button was clicked.
        /// </summary>
        None,

        /// <summary>
        /// The move up button was clicked.
        /// </summary>
        MoveUp,

        /// <summary>
        /// The move down button was clicked.
        /// </summary>
        MoveDown,

        /// <summary>
        /// The delete button was clicked.
        /// </summary>
        Delete
    }
}