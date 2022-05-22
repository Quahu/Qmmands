using System;
using System.Collections.Generic;
using System.Reflection;
using Qmmands.Text.Default;

namespace Qmmands.Text;

public class TextModuleBuilder : ITextModuleBuilder
{
    public ITextModuleBuilder? Parent { get; }

    public IList<ITextCommandBuilder> Commands => _commands;

    public IList<ITextModuleBuilder> Submodules => _submodules;

    public IList<string> Aliases { get; } = new List<string>();

    public string? Name { get; set; }

    public string? Description { get; set; }

    public IList<ICheck> Checks { get; } = new List<ICheck>();

    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

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

    public virtual ITextModule Build(ITextModule? parent = null)
    {
        return new TextModule(parent, this);
    }
}
