using System;
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
public abstract class PrimitiveTypeParserBase<T> : TypeParser<T>
{
    /// <summary>
    ///     Instantiates a new <see cref="PrimitiveTypeParserBase{T}"/>.
    /// </summary>
    protected PrimitiveTypeParserBase()
    { }

    /// <summary>
    ///     Attempts to parse the input value to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value"> The value to parse. </param>
    /// <param name="parsedValue"> The parsed value. </param>
    /// <returns>
    ///     <see langword="true"/> if the parse succeeds.
    /// </returns>
    public abstract bool TryParse(ReadOnlySpan<char> value, out T parsedValue);

    /// <inheritdoc/>
    public override ValueTask<ITypeParserResult<T>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
    {
        if (TryParse(value.Span, out var parsedValue))
            return Success(parsedValue);

        return Failure();
    }
}
