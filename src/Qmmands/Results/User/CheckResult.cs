using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="CheckAttribute"/>'s result.
    /// </summary>
    public class CheckResult : IResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public virtual bool IsSuccessful => FailureReason == null;

        /// <summary>
        ///     Gets the failure reason.
        /// </summary>
        public virtual string FailureReason { get; }

        /// <summary>
        ///     Initialises a new successful <see cref="CheckResult"/>.
        /// </summary>
        public CheckResult()
        { }

        /// <summary>
        ///     Initialises a new failed <see cref="CheckResult"/> with the specified error reason.
        /// </summary>
        /// <param name="failureReason"> The failure reason. </param>
        public CheckResult(string failureReason)
        {
            FailureReason = failureReason;
        }

        /// <summary>
        ///     Gets a successful <see cref="CheckResult"/>.
        /// </summary>
        public static CheckResult Successful
            => new();

        /// <summary>
        ///     Initialises a new failed <see cref="CheckResult"/>.
        /// </summary>
        /// <param name="reason"> The failure reason. </param>
        /// <returns>
        ///     A failed <see cref="CheckResult"/>.
        /// </returns>
        public static CheckResult Failed(string reason)
            => new(reason);

        /// <summary>
        ///     Returns <see cref="FailureReason"/>.
        /// </summary>
        /// <returns>
        ///     The <see cref="FailureReason"/>.
        /// </returns>
        public override string ToString()
            => FailureReason;

        /// <summary>
        ///     Implicitly wraps the provided <see cref="CheckResult"/> in a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result"> The result to wrap. </param>
        public static implicit operator ValueTask<CheckResult>(CheckResult result)
            => new(result);
    }
}
