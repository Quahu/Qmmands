using System;

namespace Qmmands;

/// <summary>
///     Specifies the name of the decorated module, command or parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
public class NameAttribute : Attribute,
    IModuleBuilderAttribute<IModuleBuilder>,
    ICommandBuilderAttribute<ICommandBuilder>,
    IParameterBuilderAttribute<IParameterBuilder>
{
    /// <summary>
    ///     Gets the name of this attribute.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Instantiates a new <see cref="NameAttribute"/> with the specified name.
    /// </summary>
    /// <param name="name"> The name. </param>
    public NameAttribute(string name)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public virtual void Apply(IModuleBuilder builder)
    {
        builder.Name = Name;
    }

    /// <inheritdoc/>
    public virtual void Apply(ICommandBuilder builder)
    {
        builder.Name = Name;
    }

    /// <inheritdoc/>
    public virtual void Apply(IParameterBuilder builder)
    {
        builder.Name = Name;
    }
}
