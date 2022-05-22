using System;
using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a text-based command context.
/// </summary>
public interface ITextCommandContext : ICommandContext
{
    /// <inheritdoc cref="ICommandContext.Command"/>
    new ITextCommand? Command { get; set; }

    ICommand? ICommandContext.Command
    {
        get => Command;
        set => Command = (ITextCommand) value!;
    }

    /// <summary>
    ///     Gets or sets the input <see cref="string"/> from which <see cref="Command"/> and <see cref="RawArgumentString"/>
    ///     should be extracted.
    /// </summary>
    ReadOnlyMemory<char>? InputString { get; set; }

    /// <summary>
    ///     Gets or sets the argument <see cref="string"/> from which arguments should be extracted.
    /// </summary>
    ReadOnlyMemory<char> RawArgumentString { get; set; }

    /// <summary>
    ///     Gets or sets the alias path that matched the input.
    /// </summary>
    IEnumerable<ReadOnlyMemory<char>>? Path { get; set; }

    /// <summary>
    ///     Gets or sets whether the execution should finish or attempt to execute other overloads.
    /// </summary>
    bool IsOverloadDeterminant { get; set; }
}
