using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents an option parameter,
///     i.e. a parameter that can be specified by its <see cref="ShortNames"/> and <see cref="LongNames"/>.
/// </summary>
public interface IOptionParameter : ITextParameter
{
    /// <summary>
    ///     Gets the short names of this option parameter.
    /// </summary>
    IReadOnlyList<char> ShortNames { get; }

    /// <summary>
    ///     Gets the long names of this option parameter.
    /// </summary>
    IReadOnlyList<string> LongNames { get; }

    /// <summary>
    ///     Gets whether this option is greedy,
    ///     i.e. whether it should consume more than one word without the need for quotes.
    /// </summary>
    bool IsGreedy { get; }

    /// <summary>
    ///     Gets the group of this option.
    ///     Options sharing the same group are mutually exclusive.
    /// </summary>
    object? Group { get; }
}
