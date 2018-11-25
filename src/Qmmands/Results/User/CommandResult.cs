namespace Qmmands
{
    /// <summary>
    ///     The abstract class to use for implementing results that can be returned from <see cref="Qmmands.Command"/>s.
    /// </summary>
    public abstract class CommandResult : IResult
    {
        /// <inheritdoc />
        public abstract bool IsSuccessful { get; }

        /// <summary>
        ///     The <see cref="Qmmands.Command"/> this result was returned by.
        /// </summary>
        public Command Command { get; internal set; }
    }
}