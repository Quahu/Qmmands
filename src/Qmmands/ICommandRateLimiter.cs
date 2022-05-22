using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type responsible for managing command rate-limits.
/// </summary>
public interface ICommandRateLimiter
{
    /// <summary>
    ///     Checks whether the given context is rate-limited.
    /// </summary>
    /// <param name="context"> The context to check. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the parse operation with the result being an <see cref="IResult"/>.
    /// </returns>
    ValueTask<IResult> RateLimitAsync(ICommandContext context);
}
