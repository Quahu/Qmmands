using System;
using System.Collections.Generic;
using System.Linq;

namespace Qmmands
{
    /// <summary>
    ///     Represents the command being on a cooldown.
    /// </summary>
    public sealed class CommandOnCooldownResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string FailureReason => Cooldowns.Count == 1
            ? $"Command {Command} is on a '{Cooldowns[0].Cooldown.BucketType}' cooldown. Retry after {Cooldowns[0].RetryAfter}."
            : $"Command {Command} is on multiple cooldowns: {string.Join(", ", Cooldowns.Select(x => $"'{x.Cooldown.BucketType}' - retry after {x.RetryAfter}"))}.";

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> that is on cooldown.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Cooldown"/>s and <see cref="TimeSpan"/>s after which it is safe to retry.
        /// </summary>
        public IReadOnlyList<(Cooldown Cooldown, TimeSpan RetryAfter)> Cooldowns { get; }

        internal CommandOnCooldownResult(Command command, IReadOnlyList<(Cooldown, TimeSpan)> cooldowns)
        {
            Command = command;
            Cooldowns = cooldowns;
        }
    }
}
