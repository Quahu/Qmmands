namespace Qmmands
{
    /// <summary>
    ///     Represents the reason why the argument parsing failed.
    /// </summary>
    public enum ArgumentParserFailure
    {
        /// <summary>
        ///     The <see cref="IArgumentParser"/> wasn't able to find a matching closing quote.
        /// </summary>
        UnclosedQuote,

        /// <summary>
        ///     The <see cref="IArgumentParser"/> encountered an unexpected quote.
        /// </summary>
        UnexpectedQuote,

        /// <summary>
        ///     The <see cref="IArgumentParser"/> was unable to parse some arguments as there was no whitespace between them.
        /// </summary>
        NoWhitespaceBetweenArguments,

        /// <summary>
        ///     The <see cref="IArgumentParser"/> found too few arguments.
        /// </summary>
        TooFewArguments,

        /// <summary>
        ///     The <see cref="IArgumentParser"/> found too many arguments.
        /// </summary>
        TooManyArguments
    }
}