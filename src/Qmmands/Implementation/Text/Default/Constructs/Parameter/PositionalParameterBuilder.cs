using System;
using System.Reflection;

namespace Qmmands.Text;

public class PositionalParameterBuilder : TextParameterBuilder, IPositionalParameterBuilder
{
    public bool IsRemainder { get; set; }

    public PositionalParameterBuilder(ITextCommandBuilder command, Type reflectedType)
        : base(command, reflectedType)
    { }

    public PositionalParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
        : base(command, parameterInfo)
    { }

    public override ITextParameter Build(ITextCommand command)
    {
        return new PositionalParameter(command, this);
    }
}
