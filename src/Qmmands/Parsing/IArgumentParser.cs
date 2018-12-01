namespace Qmmands
{
    /// <summary>
    ///     The default interface for raw argument parsers.
    /// </summary>
    public interface IArgumentParser
    {
        /// <summary>
        ///     Attempts to parse raw arguments for the specified <see cref="Command"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to parse raw arguments for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <returns>
        ///     An <see cref="ArgumentParserResult"/>.
        /// </returns>
        ArgumentParserResult ParseRawArguments(Command command, string rawArguments);
    }
}
