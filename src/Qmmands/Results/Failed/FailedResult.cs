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
        ///     Gets the reason of this failed result.
        /// </summary>
        public abstract string Reason { get; }

        /// <summary>
        ///     Returns <see cref="Reason"/>.
        /// </summary>
        /// <returns>
        ///     The <see cref="Reason"/>.
        /// </returns>
        public override string ToString()
            => Reason;
    }
}
