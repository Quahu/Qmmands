using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents the callback that will be executed
/// </summary>
public interface ICommandCallback
{
    /// <summary>
    ///     Creates the <see cref="IModuleBase"/> instance.
    /// </summary>
    /// <param name="context"> The execution context. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the work with the result being the created <see cref="IModuleBase"/>
    ///     or <see langword="null"/> if the module is not tied to an <see cref="IModuleBase"/>.
    /// </returns>
    ValueTask<IModuleBase?> CreateModuleBase(ICommandContext context);

    /// <summary>
    ///     Executes the command of the given execution context.
    /// </summary>
    /// <param name="context"> The execution context. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the work with the result being the <see cref="IResult"/> of execution.
    /// </returns>
    ValueTask<IResult?> ExecuteAsync(ICommandContext context);
}
