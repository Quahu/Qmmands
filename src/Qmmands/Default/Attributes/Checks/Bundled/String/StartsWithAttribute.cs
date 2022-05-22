﻿using System;
using System.Globalization;
using Qmmands.Default;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided string argument starts with the provided string value.
/// </summary>
public class StartsWithAttribute : StringConstraintParameterCheckAttribute
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
    ///     Instantiates a new <see cref="StartsWithAttribute"/> with the specified string value and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"> The string value. </param>
    public StartsWithAttribute(string value)
    {
        Value = value;
        Comparison = StringComparison.OrdinalIgnoreCase;
    }

    /// <inheritdoc/>
    protected override IResult CheckValue(CultureInfo locale, ReadOnlyMemory<char> value, bool isEnumerable)
    {
        if (value.Span.StartsWith(Value, Comparison))
            return Results.Success;

        return Results.Failure($"The provided {(isEnumerable ? "arguments" : "argument")} must start with the {(CheckUtilities.IsCaseSensitive(Comparison) ? "case-sensitive" : "case-insensitive")} value: '{Value}'.");
    }
}
