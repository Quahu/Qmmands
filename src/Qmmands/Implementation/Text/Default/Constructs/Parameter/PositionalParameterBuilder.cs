using System;
using System.Reflection;

namespace Qmmands.Text;

/// <inheritdoc cref="IPositionalParameterBuilder"/>
public class PositionalParameterBuilder : TextParameterBuilder, IPositionalParameterBuilder
{
    /// <inheritdoc/>
    public bool IsRemainder { get; set; }

    public PositionalParameterBuilder(ITextCommandBuilder command, Type reflectedType)
        : base(command, reflectedType)
    { }

    public PositionalParameterBuilder(ITextCommandBuilder command, ParameterInfo parameterInfo)
        : base(command, parameterInfo)
    { }

    /// <inheritdoc/>
    public override ITextParameter Build(ITextCommand command)
    {
        return new PositionalParameter(command, this);
    }
}
