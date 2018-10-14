using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents no overloads matching the input.
    /// </summary>
    public sealed class OverloadNotFoundResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason => "Failed to find a matching overload.";

        /// <summary>
        ///     Gets the failed overloads with their respective failure reasons.
        /// </summary>
        public IReadOnlyDictionary<Command, FailedResult> FailedOverloads { get; }

        internal OverloadNotFoundResult(IReadOnlyDictionary<Command, FailedResult> failedOverloads)
            => FailedOverloads = failedOverloads;
    }
}
