using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents errors that occur during building <see cref="Command"/>s.
    /// </summary>
    public sealed class CommandBuildingException : Exception
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.CommandBuilder"/> that failed to build.
        /// </summary>
        public CommandBuilder CommandBuilder { get; }

        internal CommandBuildingException(CommandBuilder commandBuilder, string message) : base(message)
            => CommandBuilder = commandBuilder;
    }
}
