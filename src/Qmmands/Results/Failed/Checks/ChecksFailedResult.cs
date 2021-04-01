using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Command"/>'s or a <see cref="Qmmands.Module"/>'s checks failure.
    /// </summary>
    public sealed class ChecksFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string FailureReason => $"{(FailedChecks.Count == 1 ? "One check" : "Multiple checks")} failed for the {(Module != null ? $"module {Module}" : $"command {Command}")}.";

        /// <summary>
        ///     Gets the <see cref="Qmmands.Module"/> the checks failed on.
        ///     Returns <see langword="null"/> if <see cref="Command"/> has a value.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the checks failed on.
        ///     Returns <see langword="null"/> if <see cref="Module"/> has a value.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the checks that failed with their respective results.
        /// </summary>
        public IReadOnlyList<(CheckAttribute Check, CheckResult Result)> FailedChecks { get; }

        internal ChecksFailedResult(Command command, IReadOnlyList<(CheckAttribute Check, CheckResult Result)> failedChecks)
        {
            Command = command;
            FailedChecks = failedChecks;
        }

        internal ChecksFailedResult(Module module, IReadOnlyList<(CheckAttribute Check, CheckResult Result)> failedChecks)
        {
            Module = module;
            FailedChecks = failedChecks;
        }
    }
}
