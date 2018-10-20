using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents the command being on a cooldown.
    /// </summary>
    public sealed class CommandOnCooldownResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> that's on cooldown.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="TimeSpan"/> after which it's safe to retry.
        /// </summary>
        public TimeSpan RetryAfter { get; }

        internal CommandOnCooldownResult(Command command, TimeSpan retryAfter)
        {
            Command = command;
            RetryAfter = retryAfter;
            Reason = $"Command '{command.Name}' is on cooldown with the '{command.Cooldown.BucketType}' bucket type. Retry after {retryAfter}.";
        }
    }
}
