using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents the event data for when a command errored during execution.
    /// </summary>
    public sealed class CommandExecutionFailedEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the <see cref="CommandExecutionFailedResult"/> returned from execution.
        /// </summary>
        public CommandExecutionFailedResult Result { get; }

        /// <summary>
        ///     Gets the <see cref="CommandContext"/> used for execution.
        /// </summary>
        public CommandContext Context { get; }

        /// <summary>
        ///     Initialises a new <see cref="CommandExecutionFailedEventArgs"/>.
        /// </summary>
        /// <param name="result"> The <see cref="CommandExecutionFailedResult"/> returned from execution. </param>
        /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
        public CommandExecutionFailedEventArgs(CommandExecutionFailedResult result, CommandContext context)
        {
            Result = result;
            Context = context;
        }
    }
}
