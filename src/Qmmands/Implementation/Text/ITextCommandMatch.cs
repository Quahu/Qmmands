using System;
using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a result of <see cref="ITextCommandMap.FindMatches"/>.
/// </summary>
public interface ITextCommandMatch
{
    /// <summary>
    ///     Gets the command that matches the input.
    /// </summary>
    ITextCommand Command { get; }

    /// <summary>
    ///     Gets the alias path that matched the input.
    /// </summary>
    IEnumerable<ReadOnlyMemory<char>> Path { get; }

    /// <summary>
    ///     Gets the raw arguments extracted from the input.
    /// </summary>
    ReadOnlyMemory<char> RawArgumentString { get; }
}
