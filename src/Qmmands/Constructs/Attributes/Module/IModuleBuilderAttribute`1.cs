using System;

namespace Qmmands;

/// <summary>
///     Represents an attribute that can be applied to the given builder type.
/// </summary>
public interface IModuleBuilderAttribute<in TBuilder> : IModuleBuilderAttribute
    where TBuilder : IModuleBuilder
{
    /// <summary>
    ///     Applies this attribute to the specified builder.
    /// </summary>
    /// <param name="builder"> The builder to apply the changes to. </param>
    void Apply(TBuilder builder);

    Type IModuleBuilderAttribute.BuilderType => typeof(TBuilder);

    void IModuleBuilderAttribute.Apply(object builder)
        => Apply((TBuilder) builder);
}
