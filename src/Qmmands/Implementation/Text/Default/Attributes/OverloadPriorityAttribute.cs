using System;

namespace Qmmands.Text;

/// <summary>
///     Specifies the overload priority for the decorated command.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class OverloadPriorityAttribute : Attribute,
    ICommandBuilderAttribute<ITextCommandBuilder>
{
    /// <summary>
    ///     Gets the overload priority of this attribute.
    /// </summary>
    /// <remarks>
    ///     <inheritdoc cref="ITextCommand.OverloadPriority"/>
    /// </remarks>
    public int OverloadOverloadPriority { get; }

    /// <summary>
    ///     Instantiates a new <see cref="OverloadPriorityAttribute"/> with the specified priority.
    /// </summary>
    /// <param name="overloadPriority"></param>
    public OverloadPriorityAttribute(int overloadPriority)
    {
        OverloadOverloadPriority = overloadPriority;
    }

    /// <inheritdoc/>
    public virtual void Apply(ITextCommandBuilder builder)
    {
        builder.OverloadPriority = OverloadOverloadPriority;
    }
}
