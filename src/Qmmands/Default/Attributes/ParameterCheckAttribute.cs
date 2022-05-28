using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <inheritdoc cref="IParameterCheck"/>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class ParameterCheckAttribute : Attribute, IParameterCheck,
    IParameterBuilderAttribute<IParameterBuilder>
{
    bool IParameterCheck.ChecksCollection => GetChecksCollection();

    /// <summary>
    ///     Instantiates a new <see cref="ParameterCheckAttribute"/> with the type that this check can check.
    /// </summary>
    protected ParameterCheckAttribute()
    { }

    /// <inheritdoc cref="IParameterCheck.ChecksCollection"/>
    protected virtual bool GetChecksCollection()
        => false;

    /// <inheritdoc/>
    public abstract bool CanCheck(IParameter parameter, object? value);

    /// <inheritdoc/>
    public abstract ValueTask<IResult> CheckAsync(ICommandContext context, IParameter parameter, object? value);

    /// <inheritdoc/>
    public virtual void Apply(IParameterBuilder builder)
    {
        builder.Checks.Add(this);
    }
}
