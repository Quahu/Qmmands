using System.Collections.Generic;

namespace Qmmands;

/// <summary>
///     Represents a failure of checks of a module or a command.
/// </summary>
public class ChecksFailedResult : IResult
{
    /// <summary>
    ///     Gets the reason of this failed result.
    /// </summary>
    public string FailureReason => $"{(FailedChecks.Count == 1 ? "One check" : "Multiple checks")} failed for the {(Module != null ? $"module {Module.Name}" : $"command {Command!.Name}")}.";

    bool IResult.IsSuccessful => false;

    /// <summary>
    ///     Gets the module the checks failed on.
    /// </summary>
    /// <remarks>
    ///     Returns <see langword="null"/> if <see cref="Command"/> has a value.
    /// </remarks>
    public IModule? Module { get; }

    /// <summary>
    ///     Gets the command the checks failed on.
    /// </summary>
    /// <remarks>
    ///     Returns <see langword="null"/> if <see cref="Module"/> has a value.
    /// </remarks>
    public ICommand? Command { get; }

    /// <summary>
    ///     Gets the checks that failed with their error reasons.
    /// </summary>
    public IReadOnlyDictionary<ICheck, IResult> FailedChecks { get; }

    public ChecksFailedResult(ICommand command, IReadOnlyDictionary<ICheck, IResult> failedChecks)
        : this(failedChecks)
    {
        Command = command;
    }

    public ChecksFailedResult(IModule module, IReadOnlyDictionary<ICheck, IResult> failedChecks)
        : this(failedChecks)
    {
        Module = module;
    }

    protected ChecksFailedResult(IReadOnlyDictionary<ICheck, IResult> failedChecks)
    {
        FailedChecks = failedChecks;
    }
}
