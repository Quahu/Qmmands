using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qommon.Collections.Proxied;

namespace Qmmands.Default;

/// <inheritdoc cref="ICommandPipeline"/>
/// <remarks>
///     If you intend to modify this instance while commands can possibly be executing
///     <see langword="lock"/> on the instance when modifying it.
/// </remarks>
public class DefaultCommandPipeline<TCommandContext> : ProxiedList<ICommandExecutionStep>, ICommandPipeline
    where TCommandContext : ICommandContext
{
    public override ICommandExecutionStep this[int index]
    {
        get => base[index];
        set
        {
            base[index] = value;
            _firstStep = null;
        }
    }

    private ICommandExecutionStep? _firstStep;

    public DefaultCommandPipeline()
    { }

    protected DefaultCommandPipeline(IList<ICommandExecutionStep> list)
        : base(list)
    { }

    public class EndExecutionStep : CommandExecutionStep
    {
        public static ICommandExecutionStep Instance { get; } = new EndExecutionStep();

        protected EndExecutionStep()
        { }

        protected override ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            throw new InvalidOperationException("Reached the end of the execution pipeline.");
        }
    }

    public virtual bool CanExecute(ICommandContext context)
    {
        return context is TCommandContext;
    }

    public virtual ValueTask<IResult> ExecuteAsync(ICommandContext context)
    {
        var firstStep = _firstStep;
        if (firstStep == null)
        {
            lock (this)
            {
                firstStep = _firstStep;
                if (firstStep == null)
                {
                    var nextStep = EndExecutionStep.Instance;
                    var list = List;
                    var count = list.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        var step = list[i];
                        step.Next = nextStep;
                        nextStep = step;
                    }

                    firstStep = _firstStep = nextStep;
                }
            }
        }

        return firstStep.ExecuteAsync(context);
    }

    public override bool Add(ICommandExecutionStep item)
    {
        var result = base.Add(item);
        if (result)
            _firstStep = null;

        return result;
    }

    public override bool Remove(ICommandExecutionStep item)
    {
        var result = base.Remove(item);
        if (result)
            _firstStep = null;

        return result;
    }

    public override void Insert(int index, ICommandExecutionStep item)
    {
        base.Insert(index, item);
        _firstStep = null;
    }

    public override void RemoveAt(int index)
    {
        base.RemoveAt(index);
        _firstStep = null;
    }

    public override void Clear()
    {
        base.Clear();
        _firstStep = null;
    }
}
