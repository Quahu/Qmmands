using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="command"> The currently executed <see cref="Command"/>. </param>
    /// <param name="arguments"> The parsed arguments. </param>
    /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    public delegate Task<IResult> CommandCallbackDelegate(Command command, object[] arguments, ICommandContext context, IServiceProvider provider);

    /// <summary>
    ///     Represents a <see cref="Cooldown"/> bucket key generator callback method.
    /// </summary>
    /// <param name="command"> The <see cref="Command"/> to generate the bucket key for. </param>
    /// <param name="bucketType"> The <see langword="enum"/> bucket type. </param>
    /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="Cooldown"/> bucket key.
    /// </returns>
    public delegate object CooldownBucketKeyGeneratorDelegate(Command command, object bucketType, ICommandContext context, IServiceProvider provider);

    /// <summary>
    ///     Represents a <see cref="CommandService.CommandExecuted"/> callback method.
    /// </summary>
    /// <param name="command"> The executed <see cref="Command"/>. </param>
    /// <param name="result"> The <see cref="CommandResult"/> of the command. <see langword="null"/> if the <see cref="Command"/> did not return anything. </param>
    /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    public delegate Task CommandExecutedDelegate(Command command, CommandResult result, ICommandContext context, IServiceProvider provider);

    /// <summary>
    ///     Represents a <see cref="CommandService.CommandErrored"/> callback method.
    /// </summary>
    /// <param name="result"> The <see cref="ExecutionFailedResult"/> returned from execution. </param>
    /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    public delegate Task CommandErroredDelegate(ExecutionFailedResult result, ICommandContext context, IServiceProvider provider);

    internal delegate bool TryParseDelegate<T>(string value, out T result);
}
