using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a result of an operation.
/// </summary>
/// <remarks>
///     Implements <see cref="IMetadata"/>, allowing for easy attachment of extra data
///     to existing <see cref="IResult"/> implementations.
/// </remarks>
public interface IResult : IMetadata
{
    /// <summary>
    ///     Gets whether this result represents a success.
    /// </summary>
    bool IsSuccessful { get; }

    /// <summary>
    ///     Gets the failure reason of this result.
    /// </summary>
    string? FailureReason { get; }
}