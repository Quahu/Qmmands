using System;
using System.Collections.Generic;
using System.Reflection;
using Qmmands.Text.Default;

namespace Qmmands.Text;

public class TextCommandBuilder : ITextCommandBuilder
{
    public ITextModuleBuilder Module { get; }

    public IList<ITextParameterBuilder> Parameters => _parameters;

    public IList<string> Aliases { get; } = new List<string>();

    public int OverloadPriority { get; set; }

    public bool IgnoresExtraArguments { get; set; }

    public Type? CustomArgumentParserType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public IList<ICheck> Checks { get; } = new List<ICheck>();

    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

    public MethodInfo? MethodInfo { get; }

    public ICommandCallback Callback { get; }

    IList<IParameterBuilder> ICommandBuilder.Parameters => _parameters;

    private readonly UpcastingList<ITextParameterBuilder, IParameterBuilder> _parameters = new();

    public TextCommandBuilder(ITextModuleBuilder module, ICommandCallback callback)
    {
        Module = module;
        Callback = callback;
    }

    public TextCommandBuilder(ITextModuleBuilder module, MethodInfo methodInfo, ICommandCallback callback)
        : this(module, callback)
    {
        MethodInfo = methodInfo;
    }

    public virtual ITextCommand Build(ITextModule module)
    {
        return new TextCommand(module, this);
    }
}
