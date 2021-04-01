﻿namespace Qmmands
{
    /// <summary>
    ///     Represents the command being disabled.
    /// </summary>
    public sealed class CommandDisabledResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string FailureReason => $"Command {Command} is disabled.";

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> that is disabled.
        /// </summary>
        public Command Command { get; }

        internal CommandDisabledResult(Command command)
        {
            Command = command;
        }
    }
}
