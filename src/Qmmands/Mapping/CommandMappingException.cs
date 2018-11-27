using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents errors that occur during mapping commands.
    /// </summary>
    public sealed class CommandMappingException : Exception
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> this exception occurred for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the segment to map this exception occurred for.
        ///     <see langword="null"/> if there were no segments (an attempt was made to map an ungrouped <see cref="Qmmands.Command"/> without any aliases)
        /// </summary>
        public string Segment { get; }

        internal CommandMappingException(Command command, string segment, string message) : base(message)
        {
            Command = command;
            Segment = segment;
        }
    }
}
