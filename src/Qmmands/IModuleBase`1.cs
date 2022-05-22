using System;

namespace Qmmands;

/// <inheritdoc/>
/// <typeparam name="TCommandContext"> The type of the command context. </typeparam>
public interface IModuleBase<TCommandContext> : IModuleBase
    where TCommandContext : ICommandContext
{
    /// <inheritdoc cref="IModuleBase.Context"/>
    new TCommandContext Context { get; set; }

    ICommandContext IModuleBase.Context
    {
        get => Context;
        set
        {
            if (value is not TCommandContext context)
                throw new CommandContextTypeMismatchException(typeof(TCommandContext), value.GetType(), GetType());

            Context = context;
        }
    }

    Type IModuleBase.ContextType => typeof(TCommandContext);
}
