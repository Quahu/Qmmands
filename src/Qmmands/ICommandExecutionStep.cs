using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents an execution step in the command execution pipeline.
/// </summary>
public interface ICommandExecutionStep
{
    /// <summary>
    ///     Gets the next command execution step in the pipeline.
    /// </summary>
    /// <remarks>
    ///     Can be <see langword="null"/> if the pipeline has not assigned a value to it yet.
    /// </remarks>
    ICommandExecutionStep Next { get; set; }

    /// <summary>
    ///     Executes this command execution step for the specified command context.
    /// </summary>
    /// <param name="context"> The command context to execute. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the execution step operation with the result being an <see cref="IResult"/>.
    /// </returns>
    ValueTask<IResult> ExecuteAsync(ICommandContext context);
}
