using System.Collections.Generic;

namespace Qmmands;

/// <summary>
///     Represents a failure of checks of a parameter.
/// </summary>
public class ParameterChecksFailedResult : IResult
{
    /// <summary>
    ///     Gets the reason of this failed result.
    /// </summary>
    public string FailureReason => $"{(FailedChecks.Count == 1 ? "One check" : "Multiple checks")} failed for the parameter '{Parameter.Name}'.";

    bool IResult.IsSuccessful => false;

    /// <summary>
    ///     Gets the parameter the checks failed on.
    /// </summary>
    public IParameter Parameter { get; }

    /// <summary>
    ///     Gets the argument the checks failed on.
    /// </summary>
    public object? Argument { get; }

    /// <summary>
    ///     Gets the checks that failed with their error reasons.
    /// </summary>
    public IReadOnlyDictionary<IParameterCheck, IResult> FailedChecks { get; }

    public ParameterChecksFailedResult(IParameter parameter, object? argument, IReadOnlyDictionary<IParameterCheck, IResult> failedChecks)
    {
        Parameter = parameter;
        Argument = argument;
        FailedChecks = failedChecks;
    }
}
