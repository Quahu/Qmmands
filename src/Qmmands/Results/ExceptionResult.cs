using System;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents an unsuccessful result representing by an exception.
/// </summary>
public class ExceptionResult : FailedResult
{
    /// <inheritdoc/>
    public override string FailureReason { get; }

    /// <summary>
    ///     Gets the exception of this result.
    /// </summary>
    public Exception Exception { get; }

    public ExceptionResult(string failureReason, Exception exception)
    {
        Guard.IsNotNull(exception);

        FailureReason = failureReason;
        Exception = exception;
    }

    public static ExceptionResult FromOperation(string operation, Exception exception)
        => new($"An exception occurred while {operation}.", exception);
}