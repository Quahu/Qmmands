using System;
using System.Collections.Generic;
using Qommon.Metadata;

namespace Qmmands;

public static class Results
{
    /// <summary>
    ///     Gets the singleton successful result instance.
    /// </summary>
    /// <remarks>
    ///     The instance does not support attaching metadata.
    /// </remarks>
    /// <returns>
    ///     A singleton successful result.
    /// </returns>
    public static Result Success { get; } = new SingletonSuccessfulResultImpl();

    private sealed class SingletonSuccessfulResultImpl : Result, IMetadata
    {
        IDictionary<string, object?>? IMetadata.Metadata
        {
            get => null;
            set => throw new InvalidOperationException("The singleton successful result cannot have metadata set.");
        }

        public SingletonSuccessfulResultImpl()
        { }
    }

    /// <summary>
    ///     Returns a new unsuccessful result instance.
    /// </summary>
    /// <param name="failureReason"> The failure reason. </param>
    /// <returns>
    ///     An unsuccessful result.
    /// </returns>
    public static Result Failure(string failureReason)
        => new(failureReason);

    /// <summary>
    ///     Returns a new <see cref="ExceptionResult"/>.
    /// </summary>
    /// <param name="operation"> The operation that caused the error. </param>
    /// <param name="exception"> The exception that occurred. </param>
    /// <returns>
    ///     An unsuccessful result.
    /// </returns>
    public static ExceptionResult Exception(string operation, Exception exception)
        => ExceptionResult.FromOperation(operation, exception);
}