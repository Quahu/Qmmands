using System;

namespace Qmmands;

/// <summary>
///     Disables the decorated module/command.
/// </summary>
public class DisabledAttribute : Attribute,
    IModuleBuilderAttribute<IModuleBuilder>,
    ICommandBuilderAttribute<ICommandBuilder>
{
    /// <summary>
    ///     Gets or sets the reason for the module/command being disabled.
    /// </summary>
    public string? Reason { get; set; }

    /// <inheritdoc/>
    public virtual void Apply(IModuleBuilder builder)
    {
        builder.CustomAttributes.Add(this);
    }

    /// <inheritdoc/>
    public virtual void Apply(ICommandBuilder builder)
    {
        builder.CustomAttributes.Add(this);
    }
}
