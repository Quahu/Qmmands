﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Qommon.Collections.Synchronized;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided string argument matches the provided <see cref="Regex"/> pattern.
/// </summary>
public class RegexAttribute : StringConstraintParameterCheckAttribute
{
    /// <summary>
    ///     The regex cache.
    /// </summary>
    public static ISynchronizedDictionary<(string Pattern, RegexOptions Options), Regex> Regexes = new SynchronizedDictionary<(string Pattern, RegexOptions Options), Regex>();

    /// <summary>
    ///     Gets the regex pattern of this attribute.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    ///     Gets the regex options of this attribute.
    /// </summary>
    public RegexOptions Options { get; set; } = RegexOptions.CultureInvariant;

    /// <summary>
    ///     Instantiates a new <see cref="RegexAttribute"/> with the specified <see cref="Regex"/> pattern.
    /// </summary>
    /// <param name="pattern"> The <see cref="Regex"/> pattern. </param>
    public RegexAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <inheritdoc/>
    protected override IResult CheckValue(CultureInfo locale, ReadOnlyMemory<char> value, bool isEnumerable)
    {
        var regex = Regexes.GetOrAdd((Pattern, Options), tuple => new Regex(tuple.Pattern, tuple.Options));
        var match = MemoryMarshal.TryGetString(value, out var stringValue, out var startIndex, out var length)
            ? regex.Match(stringValue, startIndex, length)
            : regex.Match(new string(value.Span));

        if (match.Success)
            return Results.Success;

        return Results.Failure($"The provided {(isEnumerable ? "arguments" : "argument")} must match the regex pattern: '{Pattern}'.");
    }
}
