using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

public class RuntimeCommandCallback : ICommandCallback
{
    public Func<ICommandContext, ValueTask<IResult?>> Delegate { get; }

    public RuntimeCommandCallback(Func<ICommandContext, ValueTask<IResult?>> @delegate)
    {
        Guard.IsNotNull(@delegate);

        Delegate = @delegate;
    }

    public ValueTask<IResult?> ExecuteAsync(ICommandContext context)
    {
        return Delegate(context);
    }
}
