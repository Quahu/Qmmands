using System;

namespace Qmmands;

/// <summary>
///     Defines common results.
/// </summary>
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
    public static SuccessfulResult Success => SuccessfulResult.Instance;

    /// <summary>
    ///     Returns a new unsuccessful result instance.
    /// </summary>
    /// <param name="failureReason"> The failure reason. </param>
    /// <returns>
    ///     An unsuccessful result.
    /// </returns>
    public static Result Failure(string failureReason)
    {
        return new(failureReason);
    }

    /// <summary>
    ///     Returns a new <see cref="ExceptionResult"/>.
    /// </summary>
    /// <param name="operation"> The operation that caused the error. </param>
    /// <param name="exception"> The exception that occurred. </param>
    /// <returns>
    ///     An unsuccessful result.
    /// </returns>
    public static ExceptionResult Exception(string operation, Exception exception)
    {
        return ExceptionResult.FromOperation(operation, exception);
    }
}
