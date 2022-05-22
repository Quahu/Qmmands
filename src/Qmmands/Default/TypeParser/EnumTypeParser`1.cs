using System;

namespace Qmmands.Default;

/// <summary>
///     Represents a type parser that handles parsing of enums.
/// </summary>
/// <typeparam name="TEnum"> <inheritdoc/> </typeparam>
public class EnumTypeParser<TEnum> : PrimitiveTypeParser<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    ///     Instantiates a new <see cref="EnumTypeParser{TEnum}"/>.
    /// </summary>
    public EnumTypeParser()
        : base(Enum.TryParse)
    { }

    /// <summary>
    ///     Instantiates a new <see cref="EnumTypeParser{TEnum}"/> with the custom delegate.
    /// </summary>
    public EnumTypeParser(TryParseDelegate<TEnum> @delegate)
        : base(@delegate)
    { }
}