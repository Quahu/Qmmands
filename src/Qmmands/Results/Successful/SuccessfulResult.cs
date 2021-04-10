namespace Qmmands
{
    /// <summary>
    ///     Represents a successful result.
    /// </summary>
    public sealed class SuccessfulResult : IResult
    {
        /// <summary>
        ///     The singleton instance of <see cref="SuccessfulResult"/>.
        /// </summary>
        public static readonly SuccessfulResult Instance = new();

        /// <summary>
        ///     Gets <see langword="true"/>.
        /// </summary>
        public bool IsSuccessful => true;

        /// <summary>
        ///     Initialises a new <see cref="SuccessfulResult"/>.
        /// </summary>
        private SuccessfulResult()
        { }
    }
}
