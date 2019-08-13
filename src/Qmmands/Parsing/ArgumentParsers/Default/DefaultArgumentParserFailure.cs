namespace Qmmands
{
    /// <summary>
    ///     Represents the reason why the argument parsing failed.
    /// </summary>
    public enum DefaultArgumentParserFailure
    {
        /// <summary>
        ///     The <see cref="DefaultArgumentParser"/> was not able to find a matching closing quote.
        /// </summary>
        UnclosedQuote,

        /// <summary>
        ///     The <see cref="DefaultArgumentParser"/> encountered an unexpected quote.
        /// </summary>
        UnexpectedQuote,

        /// <summary>
        ///     The <see cref="DefaultArgumentParser"/> was unable to parse some arguments as there was no whitespace between them.
        /// </summary>
        NoWhitespaceBetweenArguments,

        /// <summary>
        ///     The <see cref="DefaultArgumentParser"/> found too few arguments.
        /// </summary>
        TooFewArguments,

        /// <summary>
        ///     The <see cref="DefaultArgumentParser"/> found too many arguments.
        /// </summary>
        TooManyArguments
    }
}