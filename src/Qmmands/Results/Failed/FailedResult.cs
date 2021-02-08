namespace Qmmands
{
    /// <summary>
    ///     Represents a generic failure.
    /// </summary>
    public abstract class FailedResult : IResult
    {
        /// <summary>
        ///     Gets <see langword="false"/>.
        /// </summary>
        public bool IsSuccessful => false;

        /// <summary>
        ///     Gets the failure reason.
        /// </summary>
        public abstract string FailureReason { get; }

        /// <summary>
        ///     Returns <see cref="FailureReason"/>.
        /// </summary>
        /// <returns>
        ///     The <see cref="FailureReason"/>.
        /// </returns>
        public override string ToString()
            => FailureReason;
    }
}
