﻿using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

public class DelegateCheck : ICheck
{
    public object? Group { get; }

    private readonly Func<ICommandContext, ValueTask<IResult>> _predicate;

    public DelegateCheck(Func<ICommandContext, bool> predicate, object? group = null)
        : this(context => new(predicate(context) ? Results.Success : Results.Failure("Failed to pass the execution check.")), group)
    { }

    public DelegateCheck(Func<ICommandContext, ValueTask<IResult>> predicate, object? group = null)
    {
        Guard.IsNotNull(predicate);

        _predicate = predicate;
        Group = group;
    }

    public ValueTask<IResult> CheckAsync(ICommandContext context)
    {
        return _predicate(context);
    }
}
