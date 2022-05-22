using System.Collections.Generic;

namespace Qmmands.Text;

public class OverloadsFailedResult : IResult
{
    public IReadOnlyDictionary<ITextCommand, IResult> FailedOverloads { get; }

    public string FailureReason => "Failed to find a matching overload.";

    bool IResult.IsSuccessful => false;

    public OverloadsFailedResult(IReadOnlyDictionary<ITextCommand, IResult> failedOverloads)
    {
        FailedOverloads = failedOverloads;
    }
}
