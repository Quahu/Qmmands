using System;
using System.Collections.Generic;
using System.Linq;

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
        public override string Reason => _lazyReason.Value;
        private readonly Lazy<string> _lazyReason;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse failed for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the raw arguments.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the result returned from <see cref="IArgumentParser.Parse"/>.
        /// </summary>
        public ArgumentParserResult ParserResult { get; }

        internal ArgumentParseFailedResult(CommandContext context, ArgumentParserResult parserResult)
        {
            Command = context.Command;
            RawArguments = context.RawArguments;
            ParserResult = parserResult;

            _lazyReason = new Lazy<string>(() => parserResult.GetFailureReason(), true);
        }
    }
}
