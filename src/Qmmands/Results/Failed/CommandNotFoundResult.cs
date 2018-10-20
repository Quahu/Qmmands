namespace Qmmands
{
    /// <summary>
    ///     Represents no commands matching the provided input.
    /// </summary>
    public sealed class CommandNotFoundResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; } = "No command found matching the provided input.";

        internal CommandNotFoundResult()
        { }
    }
}
