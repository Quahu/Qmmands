using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <inheritdoc/>
public abstract class CommandExecutionStep : ICommandExecutionStep
{
    /// <inheritdoc/>
    public ICommandExecutionStep Next
    {
        get => _next ?? throw new InvalidOperationException("The next execution step has not been set.");
        set => _next = value;
    }
    private ICommandExecutionStep? _next;

    /// <summary>
    ///     Checks whether this command execution step can be skipped.
    /// </summary>
    /// <param name="context"> The command context to check. </param>
    /// <returns>
    ///     <see langword="true"/> if this command execution step can be skipped.
    /// </returns>
    protected virtual bool CanBeSkipped(ICommandContext context)
        => false;

    /// <inheritdoc cref="ICommandExecutionStep.ExecuteAsync"/>
    protected abstract ValueTask<IResult> OnExecuted(ICommandContext context);

    /// <inheritdoc/>
    public ValueTask<IResult> ExecuteAsync(ICommandContext context)
    {
        if (CanBeSkipped(context))
            return Next.ExecuteAsync(context);

        return OnExecuted(context);
    }
}
