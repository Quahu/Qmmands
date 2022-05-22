using System;

namespace Qmmands;

[AttributeUsage(AttributeTargets.Method)]
public abstract class CommandAttribute : Attribute,
    ICommandBuilderAttribute<ICommandBuilder>
{
    public abstract void Apply(ICommandBuilder builder);
}
