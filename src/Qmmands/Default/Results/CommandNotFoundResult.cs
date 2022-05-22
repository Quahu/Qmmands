using System;
using System.Collections.Generic;
using Qommon.Metadata;

namespace Qmmands;

public class CommandNotFoundResult : IResult
{
    public static CommandNotFoundResult Instance { get; } = new();

    public string FailureReason => "No command found matching the input.";

    bool IResult.IsSuccessful => false;

    IDictionary<string, object?>? IMetadata.Metadata
    {
        get => null;
        set => throw new InvalidOperationException("The singleton command not found result cannot have metadata set.");
    }

    private CommandNotFoundResult()
    { }
}
