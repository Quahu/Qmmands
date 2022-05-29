using System;

namespace Qmmands.Text;

/// <summary>
///     Specifies the custom <see cref="ITypeParser"/> type for the decorated parameter.
/// </summary>
/// <remarks>
///     This parser has to be retrievable from the <see cref="ITypeParserProvider"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public class CustomTypeParserAttribute : Attribute,
    IParameterBuilderAttribute<IParameterBuilder>
{
    /// <summary>
    ///     Gets the custom type parser type.
    /// </summary>
    public Type CustomTypeParserType { get; }

    /// <summary>
    ///     Instantiates a new <see cref="CustomTypeParserAttribute"/> with the specified custom <see cref="ITypeParser"/> type.
    /// </summary>
    /// <param name="customTypeParserType"> The custom <see cref="ITypeParser"/> type. </param>
    public CustomTypeParserAttribute(Type customTypeParserType)
    {
        CustomTypeParserType = customTypeParserType;
    }

    /// <inheritdoc />
    public void Apply(IParameterBuilder builder)
    {
        builder.CustomTypeParserType = CustomTypeParserType;
    }
}
