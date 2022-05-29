using System;

namespace Qmmands;

/// <summary>
///     Marks the decorated method as a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class CommandAttribute : Attribute,
    ICommandBuilderAttribute<ICommandBuilder>
{
    /// <inheritdoc/>
    public abstract void Apply(ICommandBuilder builder);
}
