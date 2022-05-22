using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands.Text;

/// <inheritdoc/>
public class DefaultTextCommandMatch : ITextCommandMatch
{
    /// <inheritdoc/>
    public ITextCommand Command { get; }

    /// <inheritdoc/>
    public IEnumerable<ReadOnlyMemory<char>> Path { get; }

    /// <inheritdoc/>
    public ReadOnlyMemory<char> RawArgumentString { get; }

    public DefaultTextCommandMatch(ITextCommand command, IImmutableStack<ReadOnlyMemory<char>> path, ReadOnlyMemory<char> rawArgumentString)
    {
        Command = command;
        Path = path;
        RawArgumentString = rawArgumentString;
    }
}
