using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands
{
    /// <summary>
    ///     Represents a found <see cref="Qmmands.Command"/>, the path to it, and raw arguments.
    /// </summary>
    public sealed class CommandMatch
    {
        /// <summary>
        ///     Gets the found <see cref="Qmmands.Command"/>.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the matched alias.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        ///     Gets the path to the found <see cref="Qmmands.Command"/>.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        ///     Gets the raw arguments.
        /// </summary>
        public string RawArguments { get; }

        internal CommandMatch(Command command, string alias, IReadOnlyList<string> path, string rawArguments)
        {
            Command = command;
            Alias = alias;
            Path = path.ToImmutableArray();
            RawArguments = rawArguments;
        }
    }
}
