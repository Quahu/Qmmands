using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <inheritdoc cref="ICheck"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public abstract class CheckAttribute : Attribute, ICheck,
    IModuleBuilderAttribute<IModuleBuilder>,
    ICommandBuilderAttribute<ICommandBuilder>
{
    /// <summary>
    ///     Gets or sets the group of the check.
    /// </summary>
    public object? Group { get; set; }

    /// <inheritdoc/>
    public abstract ValueTask<IResult> CheckAsync(ICommandContext context);

    /// <inheritdoc/>
    public void Apply(IModuleBuilder builder)
    {
        builder.Checks.Add(this);
    }

    /// <inheritdoc/>
    public void Apply(ICommandBuilder builder)
    {
        builder.Checks.Add(this);
    }
}
