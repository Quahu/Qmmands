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
        ///     Gets the path to the found <see cref="Qmmands.Command"/>.
        /// </summary>
        public string[] Path { get; }

        /// <summary>
        ///     Gets the raw arguments.
        /// </summary>
        public string RawArguments { get; }

        internal CommandMatch(Command command, string[] path, string rawArguments)
        {
            Command = command;
            Path = path;
            RawArguments = rawArguments;
        }
    }
}
