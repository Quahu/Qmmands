using System;

namespace Qmmands;

/// <summary>
///     Represents an attribute that can be applied to the given builder type.
/// </summary>
public interface IParameterBuilderAttribute<in TBuilder> : IParameterBuilderAttribute
    where TBuilder : IParameterBuilder
{
    /// <summary>
    ///     Applies this attribute to the specified builder.
    /// </summary>
    /// <param name="builder"> The builder to apply the changes to. </param>
    void Apply(TBuilder builder);

    Type IParameterBuilderAttribute.BuilderType => typeof(TBuilder);

    void IParameterBuilderAttribute.Apply(object builder)
        => Apply((TBuilder) builder);
}
