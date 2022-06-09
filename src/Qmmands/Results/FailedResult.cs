namespace Qmmands;

/// <summary>
///     Represents a failed result.
/// </summary>
public abstract class FailedResult : IResult
{
    bool IResult.IsSuccessful => false;

    /// <summary>
    ///     Gets the failure reason of this result.
    /// </summary>
    public abstract string FailureReason { get; }
}
