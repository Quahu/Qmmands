using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextCommand"/>
public class TextCommand : ITextCommand
{
    /// <inheritdoc/>
    public ITextModule Module { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ITextParameter> Parameters { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases { get; }

    /// <inheritdoc/>
    public int OverloadPriority { get; }

    /// <inheritdoc/>
    public bool IgnoresExtraArguments { get; }

    /// <inheritdoc/>
    public Type? CustomArgumentParserType { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string? Description { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ICheck> Checks { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <inheritdoc/>
    public MethodInfo? MethodInfo { get; }

    /// <inheritdoc/>
    public ICommandCallback Callback { get; }

    public TextCommand(ITextModule module, ITextCommandBuilder builder)
    {
        builder.CopyMetadataTo(this);

        Module = module;

        var parameterBuilders = builder.Parameters;
        var parameterBuilderCount = parameterBuilders.Count;
        var parameters = new ITextParameter[parameterBuilderCount];
        for (var i = 0; i < parameterBuilderCount; i++)
        {
            var parameterBuilder = parameterBuilders[i];
            parameters[i] = parameterBuilder.Build(this);
        }

        Parameters = parameters;

        var builderAliases = builder.Aliases;
        var builderAliasCount = builderAliases.Count;
        var aliases = new string[builderAliasCount];
        for (var i = 0; i < builderAliasCount; i++)
        {
            var alias = builderAliases[i];
            aliases[i] = alias;
        }

        Aliases = aliases;

        OverloadPriority = builder.OverloadPriority;
        IgnoresExtraArguments = builder.IgnoresExtraArguments;
        CustomArgumentParserType = builder.CustomArgumentParserType;

        Name = builder.Name ?? this.EnumerateFullAliases().FirstOrDefault() ?? builder.MethodInfo?.Name ?? "unnamed";
        Description = builder.Description;

        var builderChecks = builder.Checks;
        var builderCheckCount = builderChecks.Count;
        var checks = new ICheck[builderCheckCount];
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

        MethodInfo = builder.MethodInfo;
        Callback = builder.Callback;
    }
}
