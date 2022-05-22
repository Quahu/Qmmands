using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands.Text;

public class TextCommand : ITextCommand
{
    public ITextModule Module { get; }

    public IReadOnlyList<ITextParameter> Parameters { get; }

    public IReadOnlyList<string> Aliases { get; }

    public int OverloadPriority { get; }

    public bool IgnoresExtraArguments { get; }

    public Type? CustomArgumentParserType { get; }

    public string Name { get; }

    public string? Description { get; }

    public IReadOnlyList<ICheck> Checks { get; }

    public IReadOnlyList<Attribute> CustomAttributes { get; }

    public MethodInfo? MethodInfo { get; }

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
