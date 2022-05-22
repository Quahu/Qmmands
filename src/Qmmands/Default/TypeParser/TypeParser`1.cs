using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Default;

/// <summary>
///     Represents an abstract <see cref="ITypeParser{T}"/> implementation with additional utility methods.
/// </summary>
/// <typeparam name="T"> <inheritdoc/> </typeparam>
public abstract class TypeParser<T> : ITypeParser<T>
{
    /// <inheritdoc/>
    public abstract ValueTask<ITypeParserResult<T>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value);

    /// <summary>
    ///     Returns a successful <see cref="TypeParserResult{T}"/>.
    /// </summary>
    /// <param name="parsedValue"> The parsed value. </param>
    /// <returns>
    ///     An <see cref="ITypeParserResult{T}"/>.
    /// </returns>
    protected virtual TypeParserResult<T> Success(Optional<T> parsedValue)
    {
        return new(parsedValue);
    }

    /// <summary>
    ///     Returns an unsuccessful <see cref="TypeParserResult{T}"/> with a generic failure reason.
    /// </summary>
    /// <returns>
    ///     An <see cref="ITypeParserResult{T}"/>.
    /// </returns>
    protected virtual TypeParserResult<T> Failure()
    {
        var failureReason = $"Failed to parse {typeof(T).Name}.";
        return Failure(failureReason);
    }

    /// <summary>
    ///     Returns an unsuccessful <see cref="TypeParserResult{T}"/>.
    /// </summary>
    /// <param name="failureReason"> The failure reason. </param>
    /// <returns>
    ///     An <see cref="ITypeParserResult{T}"/>.
    /// </returns>
    protected virtual TypeParserResult<T> Failure(string failureReason)
    {
        return new(failureReason);
    }
}
