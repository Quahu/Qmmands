using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Command"/> callback method.
    /// </summary>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    public delegate
#if NETCOREAPP
        ValueTask<IResult>
#else
        Task<IResult>
#endif
        CommandCallbackDelegate(CommandContext context, IServiceProvider provider);

    /// <summary>
    ///     Represents a <see cref="Cooldown"/> bucket key generator callback method.
    /// </summary>
    /// <param name="bucketType"> The <see langword="enum"/> bucket type. </param>
    /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    /// <returns>
    ///     A <see cref="Cooldown"/> bucket key.
    /// </returns>
    public delegate object CooldownBucketKeyGeneratorDelegate(object bucketType, CommandContext context, IServiceProvider provider);

    internal delegate bool TryParseDelegate<T>(
#if NETCOREAPP
        ReadOnlySpan<char> value,
#else
        string value,
#endif
        out T result);
}
