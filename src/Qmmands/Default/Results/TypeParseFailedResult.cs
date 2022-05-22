using System;

namespace Qmmands;

public class TypeParseFailedResult : IResult
{
    /// <summary>
    ///     Gets the reason of this failed result.
    /// </summary>
    public string FailureReason => _reason ?? GetDefaultReason();

    bool IResult.IsSuccessful => false;

    /// <summary>
    ///     Gets the parameter the parse failed for.
    /// </summary>
    public IParameter Parameter { get; }

    /// <summary>
    ///     Gets the value passed to the type parser.
    /// </summary>
    public ReadOnlyMemory<char> Value { get; }

    private readonly string? _reason;

    public TypeParseFailedResult(IParameter parameter, ReadOnlyMemory<char> value, string? reason = null)
    {
        Parameter = parameter;
        Value = value;
        _reason = reason;
    }

    private string GetDefaultReason()
    {
        var actualType = Parameter.GetTypeInformation().ActualType;
        var nullableUnderylingType = Nullable.GetUnderlyingType(actualType);
        var friendlyName = nullableUnderylingType == null
            ? CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(actualType, out var name)
                ? name
                : actualType.Name
            : CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(nullableUnderylingType, out name)
                ? $"nullable {name}"
                : $"nullable {nullableUnderylingType.Name}";

        return $"Failed to parse the {friendlyName}.";
    }
}
