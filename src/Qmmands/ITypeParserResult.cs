using System;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents the result of a type parser.
/// </summary>
public interface ITypeParserResult : IResult
{
    /// <summary>
    ///     Gets the type of the parsed value.
    /// </summary>
    Type ParsedType { get; }

    /// <summary>
    ///     Gets the parsed value.
    /// </summary>
    Optional<object?> ParsedValue { get; }
}