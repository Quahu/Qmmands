using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;
using Qommon.Collections;

namespace Qmmands;

/// <summary>
///     Represents extension methods for <see cref="ICommand"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandExtensions
{
    public static ValueTask<IResult> ExecuteAsync(this ICommand command, ICommandContext context, params MultiString[] rawArguments)
        => command.ExecuteAsync(context, command.Parameters.Zip(rawArguments, KeyValuePair.Create).ToDictionary());

    public static ValueTask<IResult> ExecuteAsync(this ICommand command, ICommandContext context, IReadOnlyDictionary<IParameter, MultiString> rawArguments)
    {
        var dictionary = context.RawArguments ?? new Dictionary<IParameter, MultiString>();
        var parameters = command.Parameters;
        var count = parameters.Count;

        if (rawArguments.Count > count)
            throw new ArgumentException("Too many raw arguments provided.", nameof(rawArguments));

        for (var i = 0; i < count; i++)
        {
            var parameter = parameters[i];
            if (!rawArguments.TryGetValue(parameter, out var rawArgument))
                continue;

            dictionary[parameter] = rawArgument;
        }

        context.RawArguments = dictionary;
        return command.ExecuteAsync(context);
    }

    public static ValueTask<IResult> ExecuteAsync(this ICommand command, ICommandContext context, params object?[] arguments)
        => command.ExecuteAsync(context, command.Parameters.Zip(arguments, KeyValuePair.Create).ToDictionary());

    public static ValueTask<IResult> ExecuteAsync(this ICommand command, ICommandContext context, IReadOnlyDictionary<IParameter, object?> arguments)
    {
        var dictionary = context.Arguments ?? new Dictionary<IParameter, object?>();
        var parameters = command.Parameters;
        var count = parameters.Count;

        if (arguments.Count > count)
            throw new ArgumentException("Too many arguments provided.", nameof(arguments));

        for (var i = 0; i < count; i++)
        {
            var parameter = parameters[i];
            if (!arguments.TryGetValue(parameter, out var argument))
                continue;

            dictionary[parameter] = argument;
        }

        context.Arguments = dictionary;
        return command.ExecuteAsync(context);
    }

    public static ValueTask<IResult> ExecuteAsync(this ICommand command, ICommandContext context)
    {
        context.Command = command;
        return context.Services.GetRequiredService<ICommandService>().ExecuteAsync(context);
    }
}
