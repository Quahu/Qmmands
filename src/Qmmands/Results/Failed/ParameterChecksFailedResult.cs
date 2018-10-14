using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Parameter"/>'s checks failure.
    /// </summary>
    public sealed class ParameterChecksFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the checks failed on.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the checks that failed with their error reasons.
        /// </summary>
        public IReadOnlyList<(ParameterCheckBaseAttribute Check, string Error)> FailedChecks { get; }

        /// <inheritdoc />
        public override string Reason => $"One or more checks failed for parameter {Parameter.Name} in command {Parameter.Command.Name}.";

        internal ParameterChecksFailedResult(Parameter parameter, IReadOnlyList<(ParameterCheckBaseAttribute Check, string Error)> failedChecks)
        {
            Parameter = parameter;
            FailedChecks = failedChecks;
        }
    }
}
