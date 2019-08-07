using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     The abstract class to use for implementing results that can be returned from <see cref="Qmmands.Command"/>s.
    /// </summary>
    public abstract class CommandResult : IResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public abstract bool IsSuccessful { get; }

        /// <summary>
        ///     The <see cref="Qmmands.Command"/> this result was returned by.
        /// </summary>
        public Command Command { get; internal set; }

        /// <summary>
        ///     Implicitly wraps the provided <see cref="CommandResult"/> in a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result"> The result to wrap. </param>
        public static implicit operator ValueTask<CommandResult>(CommandResult result)
            => new ValueTask<CommandResult>(result);
    }
}