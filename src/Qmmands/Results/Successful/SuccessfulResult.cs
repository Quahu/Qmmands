namespace Qmmands
{
    /// <summary>
    ///     Represents a successful outcome.
    /// </summary>
    public sealed class SuccessfulResult : IResult
    {
        /// <summary>
        ///     Gets <see langword="true"/>.
        /// </summary>
        public bool IsSuccessful => true;

        /// <summary>
        ///     Initialises a new <see cref="SuccessfulResult"/>.
        /// </summary>
        public SuccessfulResult()
        { }
    }
}
