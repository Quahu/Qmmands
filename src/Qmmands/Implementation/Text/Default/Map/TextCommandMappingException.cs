using System;

namespace Qmmands.Text
{
    // TODO: bad docs
    /// <summary>
    ///     Represents errors that occur during mapping commands.
    /// </summary>
    public class TextCommandMappingException : Exception
    {
        /// <summary>
        ///     Gets the command this exception occurred for.
        /// </summary>
        public ITextCommand Command { get; }

        /// <summary>
        ///     Gets the segment to map this exception occurred for.
        /// </summary>
        /// <returns>
        ///     Empty if there was no segment to map (an attempt was made to map an ungrouped command without any aliases).
        /// </returns>
        public ReadOnlyMemory<char> Segment { get; }

        public TextCommandMappingException(ITextCommand command, ReadOnlyMemory<char> segment, string message)
            : base(message)
        {
            Command = command;
            Segment = segment;
        }
    }
}
