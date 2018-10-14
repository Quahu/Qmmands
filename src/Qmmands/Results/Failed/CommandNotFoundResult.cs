namespace Qmmands
{
    /// <summary>
    ///     Represents no commands matching the provided input.
    /// </summary>
    public class CommandNotFoundResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason => "No command found matching the provided input.";

        internal CommandNotFoundResult()
        { }
    }
}
