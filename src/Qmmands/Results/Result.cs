using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a basic implementation of <see cref="IResult"/>.
/// </summary>
/// <remarks>
///     Not all of the default result types inherit this type, some of them implement
///     <see cref="IResult"/> alone for performance reasons.
/// </remarks>
public class Result : IResult
{
    /// <inheritdoc/>
    public virtual bool IsSuccessful => string.IsNullOrEmpty(FailureReason);

    /// <inheritdoc/>
    public string? FailureReason { get; }

    /// <summary>
    ///     Instantiates a new <see cref="Result"/>.
    /// </summary>
    public Result()
    { }

    /// <summary>
    ///     Instantiates a new <see cref="Result"/> with the failure reason.
    /// </summary>
    public Result(string failureReason)
    {
        FailureReason = failureReason;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsSuccessful)
            return GetType().Name;

        return $"{GetType().Name}{{Failure: '{FailureReason}'}}";
    }

    /// <summary>
    ///     Implicitly wraps the result into a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="value"> The result to wrap. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> wrapping the result.
    /// </returns>
    public static implicit operator ValueTask<IResult>(Result value)
        => new(value);
}