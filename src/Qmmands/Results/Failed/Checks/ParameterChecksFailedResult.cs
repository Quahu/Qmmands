using System;
using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Parameter"/>'s checks failure.
    /// </summary>
    public sealed class ParameterChecksFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string Reason => _lazyReason.Value;
        private readonly Lazy<string> _lazyReason;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the checks failed on.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the argument the checks failed on.
        /// </summary>
        public object Argument { get; }

        /// <summary>
        ///     Gets the checks that failed with their error reasons.
        /// </summary>
        public IReadOnlyList<(ParameterCheckAttribute Check, CheckResult Result)> FailedChecks { get; }

        internal ParameterChecksFailedResult(Parameter parameter, object argument, IReadOnlyList<(ParameterCheckAttribute Check, CheckResult Result)> failedChecks)
        {
            Parameter = parameter;
            Argument = argument;
            FailedChecks = failedChecks;
            _lazyReason = new Lazy<string>(
                () => $"{(FailedChecks.Count == 1 ? "One check" : "Multiple checks")} failed for the parameter {Parameter.Name} in the command {Parameter.Command}.", true);
        }
    }
}
