namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="CheckBaseAttribute"/> result.
    /// </summary>
    public struct CheckResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccessful => Reason == null;

        /// <summary>
        ///     Gets the error reason.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        ///     Initialises a new <see cref="CheckResult"/> with the specified error reason.
        /// </summary>
        /// <param name="reason"> The error reason. </param>
        public CheckResult(string reason)
            => Reason = reason;

        /// <summary>
        ///     Gets a successful <see cref="CheckResult"/>.
        /// </summary>
        public static CheckResult Successful { get; } = new CheckResult();

        /// <summary>
        ///     Initialises a new unsuccessful <see cref="CheckResult"/>.
        /// </summary>
        /// <param name="reason"> The error reason. </param>
        /// <returns>
        ///     An unsuccessful <see cref="CheckResult"/>.
        /// </returns>
        public static CheckResult Unsuccessful(string reason)
            => new CheckResult(reason);
    }
}
