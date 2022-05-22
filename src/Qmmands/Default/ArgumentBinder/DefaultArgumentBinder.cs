using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qommon;
using Qommon.Collections.Synchronized;

namespace Qmmands.Default;

public class DefaultArgumentBinder : IArgumentBinder
{
    public IDictionary<Type, IArgumentResolver> Resolvers { get; }

    public DefaultArgumentBinder()
    {
        Resolvers = new SynchronizedDictionary<Type, IArgumentResolver>();
    }

    public async ValueTask<IResult> BindAsync(ICommandContext context)
    {
        Guard.IsNotNull(context.Command);

        var command = context.Command;
        var arguments = context.Arguments;
        if (arguments != null)
        {
            foreach (var (parameter, argument) in arguments)
            {
                if (argument != null && !parameter.ReflectedType.IsInstanceOfType(argument))
                    throw new InvalidOperationException($"Value of type {argument.GetType()} is not assignable to the parameter {parameter.Name} ({parameter.ReflectedType}).");
            }
        }

        var missingParameters = arguments != null ? command.Parameters.Except(arguments.Keys).ToList() : command.Parameters.ToList();
        var missingParameterCount = missingParameters.Count;
        if (missingParameterCount != 0)
        {
            // TODO: resolving
            // TODO: optional vs default value
            var optionalParameters = missingParameters.FindAll(parameter => parameter.DefaultValue.HasValue);
            if (optionalParameters.Count > 0)
                arguments ??= new Dictionary<IParameter, object?>(missingParameterCount);

            foreach (var optionalParameter in optionalParameters)
            {
                arguments![optionalParameter] = optionalParameter.DefaultValue.Value;
            }

            missingParameterCount -= optionalParameters.Count;
            if (missingParameterCount != 0)
                return Results.Failure("Failed to resolve all missing parameters.");
        }

        if (missingParameterCount != 0)
        { }

        return Results.Success;
    }
}
