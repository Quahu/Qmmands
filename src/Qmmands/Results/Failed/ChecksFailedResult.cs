using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents <see cref="Qmmands.Command"/>/<see cref="Qmmands.Module"/> checks failing.
    /// </summary>
    public sealed class ChecksFailedResult : FailedResult
    {
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

        /// <inheritdoc />
        public override string Reason => $"One or more checks failed for {(Module != null ? $"module {Module.Name}" : $"command {Command.Name}")}.";

        internal ChecksFailedResult(Command command, IReadOnlyList<(CheckBaseAttribute Check, string Error)> failedChecks)
        {
            Command = command;
            FailedChecks = failedChecks;
        }

        internal ChecksFailedResult(Module module, IReadOnlyList<(CheckBaseAttribute Check, string Error)> failedChecks)
        {
            Module = module;
            FailedChecks = failedChecks;
        }
    }
}
