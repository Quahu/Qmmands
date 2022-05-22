using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Qmmands.Default;

/// <summary>
///     Represents a type parser that handles parsing of primitive values.
/// </summary>
/// <remarks>
///     <see cref="PrimitiveTypeParser{T}"/> is a type parser that consists of primitive parsing logic,
///     i.e. synchronous code that either succeeds or does not.
/// </remarks>
/// <typeparam name="T"> <inheritdoc/> </typeparam>
public abstract class PrimitiveNumberTypeParserBase<T> : TypeParser<T>
{
    /// <summary>
    ///     The default <see cref="NumberStyles"/> for <typeparamref name="T"/>.
    /// </summary>
    public static NumberStyles DefaultStyle;

    /// <summary>
    ///     Instantiates a new <see cref="PrimitiveNumberTypeParserBase{T}"/>.
    /// </summary>
    protected PrimitiveNumberTypeParserBase()
    { }

    /// <summary>
    ///     Attempts to parse the input value to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value"> The value to parse. </param>
    /// <param name="style"> The permitted number styles. </param>
    /// <param name="provider"> The culture-specific format provider. </param>
    /// <param name="parsedValue"> The parsed value. </param>
    /// <returns>
    ///     <see langword="true"/> if the parse succeeds.
    /// </returns>
    public abstract bool TryParse(ReadOnlySpan<char> value, NumberStyles style, IFormatProvider? provider, out T parsedValue);

    /// <inheritdoc/>
    public override ValueTask<ITypeParserResult<T>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
    {
        NumberStyles? style = null;
        var attributes = parameter.CustomAttributes;
        var attributeCount = attributes.Count;
        for (var i = 0; i < attributeCount; i++)
        {
            var attribute = attributes[i];
            if (attribute is not NumberStyleAttribute numberStyleAttribute)
                continue;

            style = numberStyleAttribute.Style;
        }

        if (TryParse(value.Span, style ?? DefaultStyle, context.Locale, out var parsedValue))
            return Success(parsedValue);

        return Failure();
    }

    static PrimitiveNumberTypeParserBase()
    {
        var type = typeof(T);
        if (type == typeof(float) || type == typeof(double) || type == typeof(Half))
        {
            DefaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;
        }
        else if (type == typeof(decimal))
        {
            DefaultStyle = NumberStyles.Number;
        }
        else
        {
            DefaultStyle = NumberStyles.Integer;
        }
    }
}
