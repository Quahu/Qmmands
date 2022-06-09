using System;
using System.Collections.Generic;
using System.Reflection;
using Qmmands.Text.Default;

namespace Qmmands.Text;

/// <inheritdoc cref="ITextModuleBuilder"/>
public class TextCommandBuilder : ITextCommandBuilder
{
    /// <inheritdoc/>
    public ITextModuleBuilder Module { get; }

    /// <inheritdoc/>
    public IList<ITextParameterBuilder> Parameters => _parameters;

    /// <inheritdoc/>
    public IList<string> Aliases { get; } = new List<string>();

    /// <inheritdoc/>
    public int OverloadPriority { get; set; }

    /// <inheritdoc/>
    public bool IgnoresExtraArguments { get; set; }

    /// <inheritdoc/>
    public Type? CustomArgumentParserType { get; set; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public IList<ICheck> Checks { get; } = new List<ICheck>();

    /// <inheritdoc/>
    public IList<Attribute> CustomAttributes { get; } = new List<Attribute>();

    /// <inheritdoc/>
    public MethodInfo? MethodInfo { get; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual ITextCommand Build(ITextModule module)
    {
        return new TextCommand(module, this);
    }
}
