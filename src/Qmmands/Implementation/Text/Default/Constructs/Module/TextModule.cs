using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextModule"/>
public class TextModule : ITextModule
{
    /// <inheritdoc/>
    public ITextModule? Parent { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ITextModule> Submodules { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ITextCommand> Commands { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string? Description { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ICheck> Checks { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <inheritdoc/>
    public TypeInfo? TypeInfo { get; }

    public TextModule(ITextModule? parent, ITextModuleBuilder builder)
    {
        builder.CopyMetadataTo(this);

        Parent = parent;

        var builderAliases = builder.Aliases;
        var builderAliasCount = builderAliases.Count;
        var aliases = new string[builderAliasCount];
        for (var i = 0; i < builderAliasCount; i++)
        {
            var alias = builderAliases[i];
            aliases[i] = alias;
        }

        Aliases = aliases;

        var submoduleBuilders = builder.Submodules;
        var submoduleBuilderCount = submoduleBuilders.Count;
        var submodules = new ITextModule[submoduleBuilderCount];
        for (var i = 0; i < submoduleBuilderCount; i++)
        {
            var submoduleBuilder = submoduleBuilders[i];
            submodules[i] = submoduleBuilder.Build(this);
        }

        Submodules = submodules;

        var commandBuilders = builder.Commands;
        var commandBuilderCount = commandBuilders.Count;
        var commands = new ITextCommand[commandBuilderCount];
        for (var i = 0; i < commandBuilderCount; i++)
        {
            var commandBuilder = commandBuilders[i];
            commands[i] = commandBuilder.Build(this);
        }

        Commands = commands;

        Name = builder.Name ?? builder.TypeInfo?.Name ?? "unnamed";
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

        TypeInfo = builder.TypeInfo;
    }
}
