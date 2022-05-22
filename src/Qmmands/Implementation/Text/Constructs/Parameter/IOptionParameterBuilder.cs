using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a builder type for <see cref="IOptionParameter"/>.
/// </summary>
public interface IOptionParameterBuilder : ITextParameterBuilder
{
    /// <summary>
    ///     Gets or sets the short names of this option parameter.
    /// </summary>
    IList<char> ShortNames { get; set; }

    /// <summary>
    ///     Gets or sets the long names of this option parameter.
    /// </summary>
    IList<string> LongNames { get; set; }

    /// <summary>
    ///     Gets or sets whether this option is greedy,
    ///     i.e. whether it should consume more than one word without the need for quotes.
    /// </summary>
    bool IsGreedy { get; set; }

    /// <summary>
    ///     Gets or sets the group of this option.
    ///     Options sharing the same group are mutually exclusive.
    /// </summary>
    object? Group { get; set; }
}