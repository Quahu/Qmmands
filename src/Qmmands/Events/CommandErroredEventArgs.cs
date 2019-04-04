using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents the event data for when a command errored during execution.
    /// </summary>
    public sealed class CommandErroredEventArgs : EventArgs
    {
        /// <summary>
        ///     The <see cref="ExecutionFailedResult"/> returned from execution.
        /// </summary>
        public ExecutionFailedResult Result { get; }

        /// <summary>
        ///     The <see cref="CommandContext"/> used for execution.
        /// </summary>
        public CommandContext Context { get; }

        /// <summary>
        ///     The <see cref="IServiceProvider"/> used for execution.
        /// </summary>
        public IServiceProvider Provider { get; }

        internal CommandErroredEventArgs(ExecutionFailedResult result, CommandContext context, IServiceProvider provider)
        {
            Result = result;
            Context = context;
            Provider = provider;
        }
    }
}
