using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents the event data for when a command was executed.
    /// </summary>
    public sealed class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        ///     The <see cref="CommandResult"/> of the <see cref="Command"/>. 
        ///     <see langword="null"/> if the <see cref="Command"/> did not return anything.
        /// </summary>
        public CommandResult Result { get; }

        /// <summary>
        ///     The <see cref="CommandContext"/> used for execution.
        /// </summary>
        public CommandContext Context { get; }

        /// <summary>
        ///     The <see cref="IServiceProvider"/> used for execution.
        /// </summary>
        public IServiceProvider Provider { get; }

        internal CommandExecutedEventArgs(CommandResult result, CommandContext context, IServiceProvider provider)
        {
            Result = result;
            Context = context;
            Provider = provider;
        }
    }
}
