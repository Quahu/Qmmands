using System;

namespace Qmmands.Text;

// TODO: add module-level support?
/// <summary>
///     Specifies that the decorated text command ignores extra arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class IgnoresExtraArgumentsAttribute : Attribute,
    ICommandBuilderAttribute<ITextCommandBuilder>
{
    /// <summary>
    ///     Gets whether to ignore extra arguments.
    /// </summary>
    public bool IgnoresExtraArguments { get; }

    /// <summary>
    ///     Instantiates a new <see cref="IgnoresExtraArgumentsAttribute"/> with <see cref="IgnoresExtraArguments"/> set to <see langword="true"/>.
    /// </summary>
    public IgnoresExtraArgumentsAttribute()
    {
        IgnoresExtraArguments = true;
    }

    /// <summary>
    ///     Instantiates a new <see cref="IgnoresExtraArgumentsAttribute"/> with the specified value.
    /// </summary>
    /// <param name="ignoresExtraArguments"> The value to set. </param>
    public IgnoresExtraArgumentsAttribute(bool ignoresExtraArguments)
    {
        IgnoresExtraArguments = ignoresExtraArguments;
    }

    /// <inheritdoc />
    public void Apply(ITextCommandBuilder builder)
    {
        builder.IgnoresExtraArguments = IgnoresExtraArguments;
    }
}
