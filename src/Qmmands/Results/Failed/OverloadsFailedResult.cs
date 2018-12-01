using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents multiple failed overloads.
    /// </summary>
    public sealed class OverloadsFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; } = "Failed to find a matching overload.";

        /// <summary>
        ///     Gets the failed overloads with their respective failed results.
        /// </summary>
        public IReadOnlyDictionary<Command, FailedResult> FailedOverloads { get; }

        internal OverloadsFailedResult(IReadOnlyDictionary<Command, FailedResult> failedOverloads)
            => FailedOverloads = failedOverloads;
    }
}
