using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon;
using Qommon.Metadata;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextParameter"/>
public abstract class TextParameter : ITextParameter
{
    /// <inheritdoc/>
    public ITextCommand Command { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string? Description { get; }

    /// <inheritdoc/>
    public Type ReflectedType { get; }

    /// <inheritdoc/>
    public Optional<object?> DefaultValue { get; }

    /// <inheritdoc/>
    public Type? CustomTypeParserType { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IParameterCheck> Checks { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <inheritdoc/>
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
