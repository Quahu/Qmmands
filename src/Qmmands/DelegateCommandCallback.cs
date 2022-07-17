using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents an <see cref="ICommandCallback"/> backed with a <see langword="delegate"/>.
/// </summary>
public class DelegateCommandCallback : ICommandCallback
{
    /// <summary>
    ///     Gets the <see langword="delegate"/> of this callback.
    /// </summary>
    public Func<ICommandContext, ValueTask<IResult?>> Delegate { get; }

    /// <summary>
    ///     Instantiates a new <see cref="DelegateCommandCallback"/>.
    /// </summary>
    /// <param name="delegate"> The <see langword="delegate"/>. </param>
    public DelegateCommandCallback(Func<ICommandContext, ValueTask<IResult?>> @delegate)
    {
        Guard.IsNotNull(@delegate);

        Delegate = @delegate;
    }

    /// <inheritdoc />
    public virtual ValueTask<IModuleBase?> CreateModuleBase(ICommandContext context)
    {
        return new(result: null);
    }

    /// <inheritdoc/>
    public virtual ValueTask<IResult?> ExecuteAsync(ICommandContext context)
    {
        return Delegate(context);
    }
}
