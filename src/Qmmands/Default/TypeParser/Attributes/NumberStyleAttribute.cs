using System;
using System.Globalization;

namespace Qmmands.Default;

/// <summary>
///     Specifies what <see cref="NumberStyles"/> should be passed to <see cref="PrimitiveNumberTypeParserBase{T}"/>.
/// </summary>
public class NumberStyleAttribute : Attribute
{
    /// <summary>
    ///     Gets the <see cref="NumberStyles"/> of this attribute.
    /// </summary>
    public NumberStyles Style { get; }

    public NumberStyleAttribute(NumberStyles style)
    {
        Style = style;
    }
}