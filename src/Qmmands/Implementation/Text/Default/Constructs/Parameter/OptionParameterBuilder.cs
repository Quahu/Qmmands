using System;
using System.Collections.Generic;
using System.Reflection;

namespace Qmmands.Text;

public class OptionParameterBuilder : TextParameterBuilder, IOptionParameterBuilder
{
    public IList<char> ShortNames { get; set; } = new List<char>();

    public IList<string> LongNames { get; set; } = new List<string>();

    public bool IsGreedy { get; set; }

    public object? Group { get; set; }

    public OptionParameterBuilder(ITextCommandBuilder command, Type reflectedType)
        : base(command, reflectedType)
    { }

    public OptionParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
        : base(command, parameterInfo)
    { }

    public override ITextParameter Build(ITextCommand command)
    {
        return new OptionParameter(command, this);
    }
}
