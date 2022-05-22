using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon;

namespace Qmmands.Text;

public abstract class TextParameterBuilder : ITextParameterBuilder
{
    public ITextCommandBuilder Command { get; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public Type ReflectedType { get; }

    public Optional<object?> DefaultValue { get; set; }

    public Type? CustomTypeParserType { get; set; }

    public IList<IParameterCheck> Checks { get; } = new List<IParameterCheck>();

    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

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

    public abstract ITextParameter Build(ITextCommand command);
}
