using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextParameterBuilder"/>
public abstract class TextParameterBuilder : ITextParameterBuilder
{
    /// <inheritdoc/>
    public ITextCommandBuilder Command { get; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public Type ReflectedType { get; }

    /// <inheritdoc/>
    public Optional<object?> DefaultValue { get; set; }

    /// <inheritdoc/>
    public Type? CustomTypeParserType { get; set; }

    /// <inheritdoc/>
    public IList<IParameterCheck> Checks { get; } = new List<IParameterCheck>();

    /// <inheritdoc/>
    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

    /// <inheritdoc/>
    public ParameterInfo? ParameterInfo { get; }

    protected TextParameterBuilder(ITextCommandBuilder command, Type reflectedType)
    {
        Command = command;
        ReflectedType = reflectedType;
    }

    protected TextParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
        : this(command, parameterInfo.ParameterType)
    {
        ParameterInfo = parameterInfo;
        Name = parameterInfo.Name;

        if (parameterInfo.HasDefaultValue)
            DefaultValue = parameterInfo.DefaultValue;
    }

    /// <inheritdoc/>
    public abstract ITextParameter Build(ITextCommand command);
}
