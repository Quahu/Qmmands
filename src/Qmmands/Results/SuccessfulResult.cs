using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a successful result.
/// </summary>
public class SuccessfulResult : IResult
{
    public static SuccessfulResult Instance { get; } = new SingletonSuccessfulResultImpl();

    private sealed class SingletonSuccessfulResultImpl : SuccessfulResult, IMetadata
    {
        IDictionary<string, object?>? IMetadata.Metadata
        {
            get => null;
            set => throw new InvalidOperationException("The singleton successful result cannot have metadata set.");
        }
    }

    bool IResult.IsSuccessful => true;

    string? IResult.FailureReason => null;

    /// <summary>
    ///     Instantiates a new <see cref="SuccessfulResult"/>.
    /// </summary>
    public SuccessfulResult()
    { }

    /// <summary>
    ///     Implicitly wraps the result into a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="value"> The result to wrap. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> wrapping the result.
    /// </returns>
    public static implicit operator ValueTask<IResult>(SuccessfulResult value)
        => new(value);
}
