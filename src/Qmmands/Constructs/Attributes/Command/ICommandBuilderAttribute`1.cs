using System;

namespace Qmmands;

/// <summary>
///     Represents an attribute that can be applied to the given builder type.
/// </summary>
public interface ICommandBuilderAttribute<in TBuilder> : ICommandBuilderAttribute
    where TBuilder : ICommandBuilder
{
    /// <summary>
    ///     Applies this attribute to the specified builder.
    /// </summary>
    /// <param name="builder"> The builder to apply the changes to. </param>
    void Apply(TBuilder builder);

    Type ICommandBuilderAttribute.BuilderType => typeof(TBuilder);

    void ICommandBuilderAttribute.Apply(object builder)
        => Apply((TBuilder) builder);
}
