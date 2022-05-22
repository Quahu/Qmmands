using System;

namespace Qmmands;

public class CommandBuildingException : Exception
{
    public ICommandBuilder Builder { get; }

    public CommandBuildingException(ICommandBuilder builder, string message)
        : base(message)
    {
        Builder = builder;
    }
}
