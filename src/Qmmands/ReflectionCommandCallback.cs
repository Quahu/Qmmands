using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;
using Qommon.Disposal;

namespace Qmmands;

/// <summary>
///     Represents an <see cref="ICommandCallback"/> implementation using reflection.
/// </summary>
public sealed class ReflectionCommandCallback : ICommandCallback
{
    /// <summary>
    ///     Gets the singleton instance of <see cref="ReflectionCommandCallback"/>.
    /// </summary>
    public static ReflectionCommandCallback Instance { get; } = new();

    /// <summary>
    ///     Instantiates a new <see cref="ReflectionCommandCallback"/>.
    /// </summary>
    private ReflectionCommandCallback()
    { }

    /// <inheritdoc />
    public ValueTask<IModuleBase?> CreateModuleBase(ICommandContext context)
    {
        Guard.IsNotNull(context.Command);

        var command = context.Command;
        var module = command.Module;
        Guard.IsNotNull(module.TypeInfo);

        var typeInfo = module.TypeInfo;
        IModuleBase moduleBase;
        try
        {
            moduleBase = (ActivatorUtilities.CreateInstance(context.Services, typeInfo) as IModuleBase)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to instantiate module base '{typeInfo}'.", ex);
        }

        moduleBase.Context = context;
        return new(moduleBase);
    }

    /// <inheritdoc/>
    public async ValueTask<IResult?> ExecuteAsync(ICommandContext context)
    {
        Guard.IsNotNull(context.Command);

        var command = context.Command;
        Guard.IsNotNull(command.MethodInfo);

        var methodInfo = command.MethodInfo;
        Guard.IsNotNull(command.Module);

        Guard.IsNotNull(context.ModuleBase);

        var moduleBase = context.ModuleBase;
        IResult? returnResult = null;
        try
        {
            await moduleBase.OnBeforeExecuted().ConfigureAwait(false);

            var arguments = context.Arguments;
            var parameters = command.Parameters;
            var parameterCount = parameters.Count;
            object?[] argumentArray;
            if (arguments != null && arguments.Count > 0)
            {
                argumentArray = new object?[parameterCount];
                for (var i = 0; i < parameterCount; i++)
                {
                    argumentArray[i] = arguments[parameters[i]];
                }
            }
            else
            {
                argumentArray = Array.Empty<object>();
            }

            var methodResult = methodInfo.Invoke(moduleBase, BindingFlags.DoNotWrapExceptions | BindingFlags.ExactBinding, null, argumentArray, null);
            if (methodResult != null)
            {
                if (methodResult is IResult result)
                {
                    // IResult
                    returnResult = result;
                }
                else if (methodResult is ValueTask valueTask)
                {
                    // await ValueTask
                    await valueTask.ConfigureAwait(false);
                }
                else
                {
                    var type = methodResult.GetType();
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        // ValueTask<T> -> Task<T>
                        methodResult = methodResult.GetType().GetMethod("AsTask")!.Invoke(methodResult, null)!;
                        type = methodResult.GetType();
                    }

                    if (methodResult is Task taskResult)
                    {
                        // await Task
                        await taskResult.ConfigureAwait(false);

                        if (type.IsGenericType)
                        {
                            while (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Task<>))
                                type = type.BaseType!;

                            if (type.GenericTypeArguments[0].IsAssignableTo(typeof(IResult)))
                            {
                                // Task<T>.Result
                                returnResult = type.GetProperty("Result")!.GetValue(methodResult) as IResult;
                            }
                        }
                    }
                }

                await moduleBase.OnAfterExecuted().ConfigureAwait(false);
            }
        }
        finally
        {
            await RuntimeDisposal.WrapAsync(moduleBase).DisposeAsync().ConfigureAwait(false);
        }

        return returnResult ?? Results.Success;
    }
}
