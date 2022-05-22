using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    public class TypeParse : CommandExecutionStep
    {
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.Command?.Parameters.Count == 0 || context.RawArguments == null || context.RawArguments.Count == 0;
        }

        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            var typeParsers = context.Services.GetRequiredService<ITypeParserProvider>();
            var rawArguments = context.RawArguments!;
            var parameters = context.Command.Parameters;
            var parameterCount = parameters.Count;

            IDictionary<IParameter, object?> arguments;
            var hadArguments = false;
            if (context.Arguments != null)
            {
                hadArguments = true;
                arguments = context.Arguments;
            }
            else
            {
                context.Arguments = arguments = new Dictionary<IParameter, object?>(parameterCount);
            }

            for (var i = 0; i < parameterCount; i++)
            {
                var parameter = parameters[i];
                if (hadArguments && arguments.ContainsKey(parameter))
                    continue;

                var typeInformation = parameter.GetTypeInformation();
                var actualType = typeInformation.ActualType;
                if (!rawArguments.TryGetValue(parameter, out var rawArgument))
                    continue;

                object? value;
                var typeParser = typeParsers.GetParser(parameter);
                if (typeParser == null)
                {
                    if (parameter.CustomTypeParserType != null)
                        throw new InvalidOperationException($"No custom type parser found for parameter {parameter.Name} ({parameter.CustomTypeParserType}).");

                    if (typeInformation.IsStringLike)
                    {
                        if (!typeInformation.IsEnumerable)
                        {
                            if (typeInformation.IsMultiString)
                            {
                                value = rawArgument;
                                continue;
                            }

                            if (rawArgument.Count > 1)
                                throw new InvalidOperationException($"Invalid multi-string argument for parameter {parameter.Name} ({actualType}); must not contain multiple strings as the parameter accepts a single value.");

                            if (rawArgument.Count == 0)
                            {
                                // No string somehow, argument parsing fault if anything.
                                value = null;
                            }
                            else
                            {
                                if (typeInformation.IsString)
                                {
                                    // Takes care of efficiency for me.
                                    value = rawArgument[0].ToString();
                                }
                                else
                                {
                                    value = rawArgument[0];
                                }
                            }
                        }
                        else
                        {
                            IList list;
                            var unwrappedType = typeInformation.UnwrapOptional();
                            var isArray = unwrappedType.IsArray;
                            if (isArray)
                            {
                                list = Array.CreateInstance(actualType, rawArgument.Count);
                            }
                            else
                            {
                                var listType = typeof(List<>).MakeGenericType(actualType);
                                if (unwrappedType.IsAssignableFrom(listType))
                                {
                                    list = (Activator.CreateInstance(listType, rawArgument.Count) as IList)!;
                                }
                                else
                                {
                                    throw new InvalidOperationException($"Unsupported enumerable type for parameter {parameter.Name} ({unwrappedType}); must be an array or list.");
                                }
                            }

                            if (typeInformation.IsString)
                            {
                                for (var j = 0; j < rawArgument.Count; j++)
                                {
                                    // Converts memory to strings.
                                    var stringRawArgument = rawArgument[j].ToString();
                                    if (isArray)
                                    {
                                        list[j] = stringRawArgument;
                                    }
                                    else
                                    {
                                        list.Add(stringRawArgument);
                                    }
                                }
                            }
                            else
                            {
                                if (isArray)
                                {
                                    rawArgument.CopyTo((list as ReadOnlyMemory<char>[])!);
                                }
                                else
                                {
                                    (list as List<ReadOnlyMemory<char>>)!.AddRange(rawArgument);
                                }
                            }

                            value = list;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"No type parser found for parameter {parameter.Name} ({actualType}.");
                    }
                }
                else
                {
                    if (rawArgument.Count == 0)
                        throw new InvalidOperationException($"The multi-string argument for parameter {parameter.Name} ({actualType}) must not be a null instance.");

                    if (!typeInformation.IsEnumerable)
                    {
                        if (rawArgument.Count > 1)
                            throw new InvalidOperationException($"The multi-string argument for parameter {parameter.Name} ({actualType}) must not contain multiple strings.");

                        var rawArgumentValue = rawArgument[0];
                        var result = await typeParser.ParseAsync(context, parameter, rawArgumentValue).ConfigureAwait(false);
                        if (!result.IsSuccessful)
                            return new TypeParseFailedResult(parameter, rawArgumentValue, result.FailureReason);

                        value = result.ParsedValue.GetValueOrDefault();
                        if (value == null && !typeInformation.AllowsNull)
                            throw new InvalidOperationException($"The parsed value returned by the type parser ({typeParser.GetType()}) for parameter {parameter.Name} ({actualType}) must not be null.");
                    }
                    else
                    {
                        IList list;
                        var enumerableType = typeInformation.UnwrapOptional();
                        var isArray = enumerableType.IsArray;
                        if (isArray)
                        {
                            list = Array.CreateInstance(actualType, rawArgument.Count);
                        }
                        else
                        {
                            var listType = typeof(List<>).MakeGenericType(actualType);
                            if (enumerableType.IsAssignableFrom(listType))
                            {
                                list = (Activator.CreateInstance(listType, rawArgument.Count) as IList)!;
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unsupported enumerable type for parameter {parameter.Name} ({enumerableType}); must be an array or list.");
                            }
                        }

                        for (var j = 0; j < rawArgument.Count; j++)
                        {
                            var rawArgumentValue = rawArgument[j];
                            var result = await typeParser.ParseAsync(context, parameter, rawArgumentValue).ConfigureAwait(false);
                            if (!result.IsSuccessful)
                                return new TypeParseFailedResult(parameter, rawArgumentValue, result.FailureReason);

                            var parsedValue = result.ParsedValue.GetValueOrDefault();
                            if (parsedValue == null && !typeInformation.AllowsNull)
                                throw new InvalidOperationException($"The parsed value returned by the type parser ({typeParser.GetType()}) at element {j} for parameter {parameter.Name} ({actualType}) must not be null.");

                            if (isArray)
                                list[j] = parsedValue;
                            else
                                list.Add(parsedValue);
                        }

                        value = list;
                    }
                }

                arguments[parameter] = typeInformation.IsOptionalType
                    ? Activator.CreateInstance(parameter.ReflectedType, value)
                    : value;
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
