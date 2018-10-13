namespace Qmmands
{
    /// <summary>
    ///     Represents the execution step at which the <see cref="ExecutionFailedResult"/> was returned.
    /// </summary>
    public enum CommandExecutionStep
    {
        /// <summary>
        ///     An exception occurred while handling command checks.
        /// </summary>
        Checks,

        /// <summary>
        ///     An exception occurred in <see cref="IArgumentParser.ParseRawArguments"/> during raw argument parsing.
        /// </summary>
        ArgumentParsing,


        /// <summary>
        ///     An exception occurred during type parsing.
        /// </summary>
        TypeParsing,

        /// <summary>
        ///     An execution occurred when executing the command.
        /// </summary>
        Command
    }
}
