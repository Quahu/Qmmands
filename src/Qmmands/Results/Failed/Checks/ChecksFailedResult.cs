using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Command"/>'s/<see cref="Qmmands.Module"/>'s checks failure.
    /// </summary>
    public sealed class ChecksFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Module"/> the checks failed on, null if <see cref="Command"/> has a value.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the checks failed on, null if <see cref="Module"/> has a value.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the checks that failed with their error reasons.
        /// </summary>
        public IReadOnlyList<(CheckBaseAttribute Check, string Error)> FailedChecks { get; }

        internal ChecksFailedResult(Command command, IReadOnlyList<(CheckBaseAttribute Check, string Error)> failedChecks)
        {
            Command = command;
            FailedChecks = failedChecks;
            Reason = $"One or more checks failed for the command '{Command.Name}'.";
        }

        internal ChecksFailedResult(Module module, IReadOnlyList<(CheckBaseAttribute Check, string Error)> failedChecks)
        {
            Module = module;
            FailedChecks = failedChecks;
            Reason = $"One or more checks failed for the module '{Module.Name}'.";
        }
    }
}
