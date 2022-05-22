using System;
using System.Globalization;
using Qmmands.Default;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided string argument ends with the provided string value.
/// </summary>
public class EndsWithAttribute : StringConstraintParameterCheckAttribute
{
    /// <summary>
    ///     Gets the required string value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Gets the <see cref="StringComparison"/> used for comparison.
    /// </summary>
    public StringComparison Comparison { get; set; }

    /// <summary>
    ///     Instantiates a new <see cref="EndsWithAttribute"/> with the specified string value and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"> The string value. </param>
    public EndsWithAttribute(string value)
    {
        Value = value;
        Comparison = StringComparison.OrdinalIgnoreCase;
    }

    /// <inheritdoc/>
    protected override IResult CheckValue(CultureInfo locale, ReadOnlyMemory<char> value, bool isEnumerable)
    {
        if (value.Span.EndsWith(Value, Comparison))
            return Results.Success;

        return Results.Failure($"The provided {(isEnumerable ? "arguments" : "argument")} must end with the {(CheckUtilities.IsCaseSensitive(Comparison) ? "case-sensitive" : "case-insensitive")} value: '{Value}'.");
    }
}
