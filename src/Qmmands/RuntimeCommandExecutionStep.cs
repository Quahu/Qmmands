using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

public class RuntimeCommandExecutionStep : CommandExecutionStep
{
    public Func<ICommandContext, ICommandExecutionStep, ValueTask<IResult>> ExecutionDelegate { get; }

    public Func<ICommandContext, bool>? SkipDelegate { get; }

    public RuntimeCommandExecutionStep(Func<ICommandContext, ICommandExecutionStep, ValueTask<IResult>> executionDelegate, Func<ICommandContext, bool>? skipDelegate = null)
    {
        Guard.IsNotNull(executionDelegate);

        ExecutionDelegate = executionDelegate;
        SkipDelegate = skipDelegate;
    }

    protected override bool CanBeSkipped(ICommandContext context)
    {
        return SkipDelegate?.Invoke(context) ?? false;
    }

    protected override ValueTask<IResult> OnExecuted(ICommandContext context)
    {
        return ExecutionDelegate(context, Next);
    }
}
