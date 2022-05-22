using System;

namespace Qmmands;

/// <summary>
///     Represents an attribute that can be applied to the given builder type.
/// </summary>
public interface IModuleBuilderAttribute
{
    /// <summary>
    ///     Gets the builder type of this attribute.
    /// </summary>
    Type BuilderType { get; }

    /// <summary>
    ///     Applies this attribute to the specified builder.
    /// </summary>
    /// <param name="builder"> The builder to apply the changes to. </param>
    void Apply(object builder);
}
