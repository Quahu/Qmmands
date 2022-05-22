using System;

namespace Qmmands;

public class ParameterBuildingException : Exception
{
    public IParameterBuilder Builder { get; }

    public ParameterBuildingException(IParameterBuilder builder, string message)
        : base(message)
    {
        Builder = builder;
    }
}
