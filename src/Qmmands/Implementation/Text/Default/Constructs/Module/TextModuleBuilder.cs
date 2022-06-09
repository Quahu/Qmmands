using System;
using System.Collections.Generic;
using System.Reflection;
using Qmmands.Text.Default;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextModuleBuilder"/>
public class TextModuleBuilder : ITextModuleBuilder
{
    /// <inheritdoc/>
    public ITextModuleBuilder? Parent { get; }

    /// <inheritdoc/>
    public IList<ITextCommandBuilder> Commands => _commands;

    /// <inheritdoc/>
    public IList<ITextModuleBuilder> Submodules => _submodules;

    /// <inheritdoc/>
    public IList<string> Aliases { get; } = new List<string>();

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public IList<ICheck> Checks { get; } = new List<ICheck>();

    /// <inheritdoc/>
    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

    /// <inheritdoc/>
    public TypeInfo? TypeInfo { get; }

    IList<ICommandBuilder> IModuleBuilder.Commands => _commands;

    IList<IModuleBuilder> IModuleBuilder.Submodules => _submodules;

    private readonly UpcastingList<ITextModuleBuilder, IModuleBuilder> _submodules = new();
    private readonly UpcastingList<ITextCommandBuilder, ICommandBuilder> _commands = new();

    public TextModuleBuilder()
        : this(null)
    { }

    public TextModuleBuilder(ITextModuleBuilder? parent)
    {
        Parent = parent;
    }

    public TextModuleBuilder(ITextModuleBuilder? parent, TypeInfo typeInfo)
        : this(parent)
    {
        TypeInfo = typeInfo;
        Name = typeInfo.Name;
    }

    /// <inheritdoc/>
    public virtual ITextModule Build(ITextModule? parent = null)
    {
        return new TextModule(parent, this);
    }
}
