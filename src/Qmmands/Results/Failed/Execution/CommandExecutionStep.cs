namespace Qmmands
{
    /// <summary>
    ///     Represents the execution step at which the <see cref="ExecutionFailedResult"/> was returned.
    /// </summary>
    public enum CommandExecutionStep
    {
        /// <summary>
        ///     An exception occurred while handling the <see cref="Qmmands.Command"/>'s checks.
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
        ///     An exception occcured in <see cref="ModuleBase{TContext}.BeforeExecutedAsync(Command)"/>.
        /// </summary>
        BeforeExecuted,

        /// <summary>
        ///     An execution occurred when executing the <see cref="Command"/>.
        /// </summary>
        Command
    }
}
