using System;
using System.Collections.Generic;
using System.Reflection;

namespace Qmmands.Text;

/// <inheritdoc cref="IOptionParameterBuilder"/>
public class OptionParameterBuilder : TextParameterBuilder, IOptionParameterBuilder
{
    /// <inheritdoc/>
    public IList<char> ShortNames { get; set; } = new List<char>();

    /// <inheritdoc/>
    public IList<string> LongNames { get; set; } = new List<string>();

    /// <inheritdoc/>
    public bool IsGreedy { get; set; }

    /// <inheritdoc/>
    public object? Group { get; set; }

    public OptionParameterBuilder(ITextCommandBuilder command, Type reflectedType)
        : base(command, reflectedType)
    { }

    public OptionParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
        : base(command, parameterInfo)
    { }

    /// <inheritdoc/>
    public override ITextParameter Build(ITextCommand command)
    {
        return new OptionParameter(command, this);
    }
}
