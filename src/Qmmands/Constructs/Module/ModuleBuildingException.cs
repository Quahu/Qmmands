using System;

namespace Qmmands;

public class ModuleBuildingException : Exception
{
    public IModuleBuilder Builder { get; }

    public ModuleBuildingException(IModuleBuilder builder, string message)
        : base(message)
    {
        Builder = builder;
    }
}
