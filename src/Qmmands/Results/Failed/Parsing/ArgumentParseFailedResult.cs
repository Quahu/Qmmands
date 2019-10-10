namespace Qmmands
{
    /// <summary>
    ///     Represents an argument parse failure.
    /// </summary>
    public sealed class ArgumentParseFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string Reason => ParserResult.Reason;
        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse failed for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the raw arguments.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the result returned from <see cref="IArgumentParser.ParseAsync"/>.
        /// </summary>
        public ArgumentParserResult ParserResult { get; }

        internal ArgumentParseFailedResult(CommandContext context, ArgumentParserResult parserResult)
        {
            Command = context.Command;
            RawArguments = context.RawArguments;
            ParserResult = parserResult;
        }
    }
}
