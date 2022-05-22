using System;
using Qommon;

namespace Qmmands;

/// <inheritdoc/>
public interface ITypeParserResult<T> : ITypeParserResult
{
    /// <summary>
    ///     Gets the parsed value.
    /// </summary>
    new Optional<T> ParsedValue { get; }

    Type ITypeParserResult.ParsedType => typeof(T);

    Optional<object?> ITypeParserResult.ParsedValue
    {
        get
        {
            var parsedValue = ParsedValue;
            if (!parsedValue.HasValue)
                return default;

            return new(parsedValue.Value);
        }
    }
}