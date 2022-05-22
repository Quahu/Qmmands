using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Default;

/// <inheritdoc cref="ITypeParserResult{T}"/>
public class TypeParserResult<T> : Result, ITypeParserResult<T>
{
    /// <inheritdoc/>
    public Optional<T> ParsedValue { get; }

    /// <summary>
    ///     Instantiates a new successful <see cref="TypeParserResult{T}"/>.
    /// </summary>
    /// <param name="parsedValue"> The parsed value. </param>
    public TypeParserResult(Optional<T> parsedValue)
    {
        ParsedValue = parsedValue;
    }

    /// <summary>
    ///     Instantiates a new unsuccessful <see cref="TypeParserResult{T}"/>.
    /// </summary>
    /// <param name="failureReason"> The failure reason. </param>
    public TypeParserResult(string failureReason)
        : base(failureReason)
    { }

    /// <summary>
    ///     Implicitly wraps the result into a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="value"> The result to wrap. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> wrapping the result.
    /// </returns>
    public static implicit operator ValueTask<ITypeParserResult<T>>(TypeParserResult<T> value)
        => new(value);
}