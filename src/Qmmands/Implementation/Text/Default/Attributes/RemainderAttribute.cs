using System;

namespace Qmmands.Text;

/// <summary>
///     Marks the decorated positional parameter as a remainder parameter,
///     i.e. sets <see cref="IPositionalParameter.IsRemainder"/> to true.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class RemainderAttribute : Attribute,
    IParameterBuilderAttribute<IPositionalParameterBuilder>
{
    /// <inheritdoc/>
    public virtual void Apply(IPositionalParameterBuilder builder)
    {
        builder.IsRemainder = true;
    }
}
