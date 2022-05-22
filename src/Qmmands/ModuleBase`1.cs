using System.Threading.Tasks;

namespace Qmmands;

public abstract class ModuleBase<TCommandContext> : IModuleBase<TCommandContext>
    where TCommandContext : ICommandContext
{
    /// <inheritdoc/>
    public TCommandContext Context { get; set; } = default!;

    /// <inheritdoc/>
    public virtual ValueTask OnBeforeExecuted()
        => default;

    /// <inheritdoc/>
    public virtual ValueTask OnAfterExecuted()
        => default;
}
