using System;
using Qommon;

namespace Qmmands.Default;

/// <summary>
///     Represents a type parser that handles parsing of primitive values.
/// </summary>
/// <remarks>
///     <see cref="PrimitiveTypeParser{T}"/> is a type parser that consists of primitive parsing logic,
///     i.e. synchronous code that either succeeds or does not.
/// </remarks>
/// <typeparam name="T"> <inheritdoc/> </typeparam>
public class PrimitiveTypeParser<T> : PrimitiveTypeParserBase<T>
{
    /// <summary>
    ///     Gets the <see cref="Delegate"/> of this parser.
    /// </summary>
    protected TryParseDelegate<T> Delegate { get; }

    /// <summary>
    ///     Instantiates a new <see cref="PrimitiveTypeParser{T}"/> with a custom delegate.
    /// </summary>
    /// <param name="delegate"> The custom delegate. </param>
    public PrimitiveTypeParser(TryParseDelegate<T> @delegate)
    {
        Guard.IsNotNull(@delegate);

        Delegate = @delegate;
    }

    /// <inheritdoc/>
    public override bool TryParse(ReadOnlySpan<char> value, out T parsedValue)
    {
        return Delegate(value, out parsedValue);
    }
}
