namespace Qmmands
{
    /// <summary>
    ///     The base interface for all results.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        bool IsSuccessful { get; }
    }
}
