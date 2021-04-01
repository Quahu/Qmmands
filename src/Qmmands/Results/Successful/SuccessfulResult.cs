namespace Qmmands
{
    /// <summary>
    ///     Represents a successful result.
    /// </summary>
    public sealed class SuccessfulResult : IResult
    {
        public static readonly SuccessfulResult Instance = new SuccessfulResult();
        
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
