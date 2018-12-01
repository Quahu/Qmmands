namespace Qmmands
{
    /// <summary>
    ///     Defines whether the <see cref="Command"/>s should run sequentially or in parallel.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        ///     <see cref="Command"/>s will run sequentially.
        /// </summary>
        Sequential,

        /// <summary>
        ///     <see cref="Command"/>s will run in parallel.
        /// </summary>
        Parallel
    }
}
