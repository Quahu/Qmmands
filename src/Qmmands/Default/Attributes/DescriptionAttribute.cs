using System;

namespace Qmmands;

/// <summary>
///     Specifies the description of the decorated member.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
public class DescriptionAttribute : Attribute,
    IModuleBuilderAttribute<IModuleBuilder>,
    ICommandBuilderAttribute<ICommandBuilder>,
    IParameterBuilderAttribute<IParameterBuilder>
{
    /// <summary>
    ///     Gets the description of this attribute.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Instantiates a new <see cref="NameAttribute"/> with the specified description.
    /// </summary>
    /// <param name="description"> The description. </param>
    public DescriptionAttribute(string description)
    {
        Description = description;
    }

    /// <inheritdoc/>
    public void Apply(IModuleBuilder builder)
    {
        builder.Description = Description;
    }

    /// <inheritdoc/>
    public void Apply(ICommandBuilder builder)
    {
        builder.Description = Description;
    }

    /// <inheritdoc/>
    public void Apply(IParameterBuilder builder)
    {
        builder.Description = Description;
    }
}
