namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="CheckBaseAttribute"/> result.
    /// </summary>
    public struct CheckResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccessful => Error == null;

        /// <summary>
        ///     Gets the error reason.
        /// </summary>
        public string Error { get; }

        /// <summary>
        ///     Initialises a new <see cref="CheckResult"/> with the specified error reason.
        /// </summary>
        /// <param name="error"> The error reason. </param>
        public CheckResult(string error) 
            => Error = error;

        /// <summary>
        ///     Gets a successful <see cref="CheckResult"/>.
        /// </summary>
        public static CheckResult Successful { get; } = new CheckResult();
    }
}
