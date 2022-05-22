using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type that validates whether the execution of the command can proceed.
/// </summary>
public interface ICheck
{
    /// <summary>
    ///     Gets the group of this check.
    /// </summary>
    object? Group { get; }

    /// <summary>
    ///     Checks whether the command context is valid for execution.
    /// </summary>
    /// <param name="context"> The command context. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the check operation with the result being an <see cref="IResult"/>.
    /// </returns>
    ValueTask<IResult> CheckAsync(ICommandContext context);
}
