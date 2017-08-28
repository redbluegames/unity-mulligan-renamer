namespace RedBlueGames.MulliganRenamer
{
    /// <summary>
    /// Operations stored by Diffs
    /// </summary>
    public enum DiffOperation
    {
        /// <summary>
        /// The string is equal to the previous value.
        /// </summary>
        Equal,

        /// <summary>
        /// The string represents a deletion to the original string.
        /// </summary>
        Deletion,

        /// <summary>
        /// The string represents an insertion to the original string.
        /// </summary>
        Insertion
    }
}