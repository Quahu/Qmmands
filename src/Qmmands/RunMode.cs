namespace Qmmands
{
    /// <summary> Defines whether the commands should run sequentially, or in parallel on different threads. </summary>
    public enum RunMode
    {
        /// <summary> Commands will run sequentially. </summary>
        Sequential,

        /// <summary> Commands will run in parallel, each on another thread. </summary>
        Parallel
    }
}
