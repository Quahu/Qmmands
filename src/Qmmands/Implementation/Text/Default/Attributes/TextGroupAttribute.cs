using System;
using Qommon.Collections;

namespace Qmmands.Text;

/// <summary>
///     Marks the decorated class as a text group module.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TextGroupAttribute : Attribute,
    IModuleBuilderAttribute<ITextModuleBuilder>
{
    /// <summary>
    ///     Gets the aliases of the group.
    /// </summary>
    public string[] Aliases { get; }

    /// <summary>
    ///     Instantiates a new <see cref="TextGroupAttribute"/> with the specified aliases.
    /// </summary>
    /// <param name="aliases"> The aliases of the group. </param>
    public TextGroupAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }

    /// <inheritdoc/>
    public virtual void Apply(ITextModuleBuilder builder)
    {
        builder.Aliases.AddRange(Aliases);
    }
}
