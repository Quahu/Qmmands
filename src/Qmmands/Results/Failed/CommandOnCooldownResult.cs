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
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> that's on cooldown.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Cooldown"/>s and <see cref="TimeSpan"/>s after which it's safe to retry.
        /// </summary>
        public IReadOnlyList<(Cooldown Cooldown, TimeSpan RetryAfter)> Cooldowns { get; }

        internal CommandOnCooldownResult(Command command, IReadOnlyList<(Cooldown Cooldown, TimeSpan RetryAfter)> cooldowns)
        {
            Command = command;
            Cooldowns = cooldowns;
            Reason = cooldowns.Count == 1
                ? $"Command '{command.Name}' is on a '{cooldowns[0].Cooldown.BucketType}' cooldown. Retry after {cooldowns[0].RetryAfter}."
                : $"Command '{command.Name}' is on multiple cooldowns: {string.Join(", ", cooldowns.Select(x => $"'{x.Cooldown.BucketType}' - retry after {x.RetryAfter}"))}";
        }
    }
}
