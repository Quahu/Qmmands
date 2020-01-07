using System;
using System.Threading.Tasks;

namespace Qmmands.Delegates
{
    /// <summary>
    ///     Represents a <see cref="void"/> <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    public delegate void VoidCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="CommandResult"/> <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="CommandResult"/>.
    /// </returns>
    public delegate CommandResult ResultCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="Task"/> <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="Task"/>.
    /// </returns>
    public delegate Task TaskCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> where TResult is a <see cref="CommandResult"/>.
    /// </returns>
    public delegate Task<CommandResult> TaskResultCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="ValueTask"/> <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="ValueTask"/>.
    /// </returns>
    public delegate ValueTask ValueTaskCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> where TResult is a <see cref="CommandResult"/>.
    /// </returns>
    public delegate ValueTask<CommandResult> ValueTaskResultCommandCallbackDelegate(CommandContext context);

    /// <summary>
    ///     Represents a <see cref="Cooldown"/> bucket key generator callback method.
    /// </summary>
    /// <param name="bucketType"> The <see langword="enum"/> bucket type. </param>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="Cooldown"/> bucket key.
    /// </returns>
    public delegate object CooldownBucketKeyGeneratorDelegate(Enum bucketType, CommandContext context);

    internal delegate Task<IResult> ModuleBaseCommandCallbackDelegate(CommandContext context);

    internal delegate bool TryParseDelegate<T>(ReadOnlySpan<char> value, out T result);
}
