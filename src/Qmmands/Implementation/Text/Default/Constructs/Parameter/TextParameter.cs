﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon;
using Qommon.Metadata;

namespace Qmmands.Text;

public abstract class TextParameter : ITextParameter
{
    public ITextCommand Command { get; }

    public string Name { get; }

    public string? Description { get; }

    public Type ReflectedType { get; }

    public Optional<object?> DefaultValue { get; }

    public Type? CustomTypeParserType { get; }

    public IReadOnlyList<IParameterCheck> Checks { get; }

    public IReadOnlyList<Attribute> CustomAttributes { get; }

    public ParameterInfo? ParameterInfo { get; }

    protected TextParameter(ITextCommand command, ITextParameterBuilder builder)
    {
        builder.CopyMetadataTo(this);

        Command = command;

        Name = builder.Name ?? builder.ParameterInfo?.Name ?? "unnamed";
        Description = builder.Description;
        ReflectedType = builder.ReflectedType;
        DefaultValue = builder.DefaultValue;
        CustomTypeParserType = builder.CustomTypeParserType;

        var builderChecks = builder.Checks;
        var builderCheckCount = builderChecks.Count;
        var checks = new IParameterCheck[builderCheckCount];
        for (var i = 0; i < builderCheckCount; i++)
        {
            var check = builderChecks[i];
            checks[i] = check;
        }

        Checks = checks;

        var builderCustomAttributes = builder.CustomAttributes;
        var builderCustomAttributeCount = builderCustomAttributes.Count;
        var customAttributes = new Attribute[builderCustomAttributeCount];
        for (var i = 0; i < builderCustomAttributeCount; i++)
        {
            var customAttribute = builderCustomAttributes[i];
            customAttributes[i] = customAttribute;
        }

        CustomAttributes = customAttributes;

        ParameterInfo = builder.ParameterInfo;
    }
}
