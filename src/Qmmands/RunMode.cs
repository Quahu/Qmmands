namespace Qmmands
{
    /// <summary>
    ///     Defines whether the commands should run sequentially or in parallel.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        ///     Commands will run sequentially.
        /// </summary>
        Sequential,

        /// <summary>
        ///     Commands will run in parallel.
        /// </summary>
        Parallel
    }
}
