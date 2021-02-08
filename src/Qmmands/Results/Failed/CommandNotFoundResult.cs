namespace Qmmands
{
    /// <summary>
    ///     Represents no commands matching the provided input.
    /// </summary>
    public sealed class CommandNotFoundResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string FailureReason => "No command found matching the provided input.";

        internal CommandNotFoundResult()
        { }
    }
}
