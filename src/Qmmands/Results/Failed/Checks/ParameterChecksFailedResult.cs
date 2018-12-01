using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Parameter"/>'s checks failure.
    /// </summary>
    public sealed class ParameterChecksFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the checks failed on.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the checks that failed with their error reasons.
        /// </summary>
        public IReadOnlyList<(ParameterCheckBaseAttribute Check, string Error)> FailedChecks { get; }

        internal ParameterChecksFailedResult(Parameter parameter, IReadOnlyList<(ParameterCheckBaseAttribute Check, string Error)> failedChecks)
        {
            Parameter = parameter;
            FailedChecks = failedChecks;
            Reason = $"{(FailedChecks.Count == 1 ? "One check" : "Multiple checks")} failed for the parameter {Parameter.Name} in the command {Parameter.Command}.";
        }
    }
}
