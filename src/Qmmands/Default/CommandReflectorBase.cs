using System;
using System.Linq;
using System.Reflection;

namespace Qmmands.Default;

public abstract class CommandReflectorBase<TModuleBuilder, TCommandBuilder, TParameterBuilder, TCommandContext>
    : ICommandReflector
    where TModuleBuilder : class, IModuleBuilder
    where TCommandBuilder : class, ICommandBuilder
    where TParameterBuilder : class, IParameterBuilder
    where TCommandContext : class, ICommandContext
{
    Type ICommandReflector.ContextType => typeof(TCommandContext);

    protected abstract TModuleBuilder CreateModuleBuilder(TModuleBuilder? parent, TypeInfo typeInfo);

    protected abstract TCommandBuilder CreateCommandBuilder(TModuleBuilder module, MethodInfo methodInfo);

    protected abstract TParameterBuilder CreateParameterBuilder(TCommandBuilder command, ParameterInfo parameterInfo);

    protected virtual void PopulateModuleBuilder(TModuleBuilder module, TypeInfo typeInfo)
    {
        ApplyModuleBuilderAttributes(module, typeInfo);

        foreach (var methodInfo in typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!methodInfo.GetCustomAttributes<CommandAttribute>(true).Any())
                continue;

            var commandBuilder = CreateCommand(module, methodInfo);
            module.Commands.Add(commandBuilder);
        }

        foreach (var nestedType in typeInfo.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public))
        {
            var nestedTypeInfo = nestedType.GetTypeInfo();
            if (!IsValidModule(nestedTypeInfo))
                continue;

            var submoduleBuilder = CreateSubmodule(module, nestedTypeInfo);
            module.Submodules.Add(submoduleBuilder);
        }
    }

    protected virtual void PopulateMethodBuilder(TCommandBuilder command, MethodInfo methodInfo)
    {
        ApplyCommandBuilderAttributes(command, methodInfo);

        foreach (var parameterInfo in methodInfo.GetParameters())
        {
            var parameterBuilder = CreateParameter(command, parameterInfo);
            command.Parameters.Add(parameterBuilder);
        }
    }

    protected virtual void PopulateParameterBuilder(TParameterBuilder parameter, ParameterInfo parameterInfo)
    {
        ApplyParameterBuilderAttributes(parameter, parameterInfo);
    }

    protected virtual void ApplyModuleBuilderAttributes(IModuleBuilder module, ICustomAttributeProvider attributeProvider)
    {
        foreach (var customAttribute in attributeProvider.GetCustomAttributes(true))
        {
            if (customAttribute is IModuleBuilderAttribute builderAttribute && builderAttribute.BuilderType.IsInstanceOfType(module))
                builderAttribute.Apply(module);
            else
                module.CustomAttributes.Add((customAttribute as Attribute)!);
        }
    }

    protected virtual void ApplyCommandBuilderAttributes(ICommandBuilder command, ICustomAttributeProvider attributeProvider)
    {
        foreach (var customAttribute in attributeProvider.GetCustomAttributes(true))
        {
            if (customAttribute is ICommandBuilderAttribute builderAttribute && builderAttribute.BuilderType.IsInstanceOfType(command))
                builderAttribute.Apply(command);
            else
                command.CustomAttributes.Add((customAttribute as Attribute)!);
        }
    }

    protected virtual void ApplyParameterBuilderAttributes(IParameterBuilder parameter, ICustomAttributeProvider attributeProvider)
    {
        foreach (var customAttribute in attributeProvider.GetCustomAttributes(true))
        {
            if (customAttribute is IParameterBuilderAttribute builderAttribute && builderAttribute.BuilderType.IsInstanceOfType(parameter))
                builderAttribute.Apply(parameter);
            else
                parameter.CustomAttributes.Add((customAttribute as Attribute)!);
        }
    }

    protected virtual void ExecuteBuilderAction(TModuleBuilder builder, Action<TModuleBuilder>? builderAction)
    {
        builderAction?.Invoke(builder);
    }

    protected virtual bool IsValidModule(TypeInfo typeInfo)
        => typeof(IModuleBase).IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters;

    protected virtual bool IsValidCommand(MethodInfo methodInfo)
        => !methodInfo.IsAbstract && !methodInfo.IsStatic && !methodInfo.ContainsGenericParameters;

    protected virtual bool IsValidParameter(ParameterInfo parameterInfo)
        => !parameterInfo.ParameterType.IsByRef;

    public virtual IModule CreateModule(TypeInfo typeInfo, Action<IModuleBuilder>? builderAction = null)
    {
        if (!IsValidModule(typeInfo))
            throw new ArgumentException($"Command module {typeInfo} is not valid.", nameof(typeInfo));

        var builder = CreateModuleBuilder(null, typeInfo);
        PopulateModuleBuilder(builder, typeInfo);
        ExecuteBuilderAction(builder, builderAction);
        return builder.Build();
    }

    public virtual TModuleBuilder CreateSubmodule(TModuleBuilder parent, TypeInfo typeInfo)
    {
        if (!IsValidModule(typeInfo))
            throw new ArgumentException($"Command submodule {typeInfo} is not valid.", nameof(typeInfo));

        var submodule = CreateModuleBuilder(parent, typeInfo);
        PopulateModuleBuilder(submodule, typeInfo);
        return submodule;
    }

    public virtual TCommandBuilder CreateCommand(TModuleBuilder module, MethodInfo methodInfo)
    {
        if (!IsValidCommand(methodInfo))
            throw new ArgumentException($"Command method {methodInfo} is not valid.", nameof(methodInfo));

        var command = CreateCommandBuilder(module, methodInfo);
        PopulateMethodBuilder(command, methodInfo);
        return command;
    }

    public virtual TParameterBuilder CreateParameter(TCommandBuilder command, ParameterInfo parameterInfo)
    {
        if (!IsValidParameter(parameterInfo))
            throw new ArgumentException($"Command parameter {parameterInfo} is not valid.", nameof(parameterInfo));

        var parameter = CreateParameterBuilder(command, parameterInfo);
        PopulateParameterBuilder(parameter, parameterInfo);
        return parameter;
    }
}
