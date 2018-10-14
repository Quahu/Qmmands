using System.Collections.Generic;
using System.Linq;

namespace Qmmands
{
    /// <summary>
    ///     Represents an argument parse failure.
    /// </summary>
    public sealed class ParseFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse failed for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed on, can be null depending on the <see cref="ParseFailure"/>.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the raw arguments passed to the <see cref="IArgumentParser.ParseRawArguments"/>.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the successfully parsed arguments.
        /// </summary>
        public IReadOnlyDictionary<Parameter, object> Arguments { get; set; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.ParseFailure"/> that occurred.
        /// </summary>
        public ParseFailure ParseFailure { get; }

        /// <summary>
        ///     Gets the position (index) at which the parsing failed, can be null depending on the <see cref="ParseFailure"/>. 
        /// </summary>
        public int? Position { get; }

        internal ParseFailedResult(Command command, ParseResult parseResult)
        {
            Command = command;
            Parameter = parseResult.Parameter;
            RawArguments = parseResult.RawArguments;
            Arguments = parseResult.Arguments;
            ParseFailure = parseResult.ParseFailure.Value;
            Position = parseResult.FailurePosition;
            switch (parseResult.ParseFailure)
            {
                case ParseFailure.UnclosedQuote:
                    Reason = "A quotation mark was left unclosed.";
                    break;

                case ParseFailure.UnexpectedQuote:
                    Reason = "Encountered an unexpected quotation mark.";
                    break;

                case ParseFailure.NoWhitespaceBetweenArguments:
                    Reason = "Whitespace is required between arguments.";
                    break;

                case ParseFailure.TooFewArguments:
                    var missingParameters = Command.Parameters.SkipWhile(x => x != Parameter).Where(x => !x.IsOptional).Select(x => $"'{x}'").ToArray();
                    Reason = $"Required {(missingParameters.Length == 1 ? "parameter" : "parameters")} " +
                             $"{string.Join(", ", missingParameters)} {(missingParameters.Length == 1 ? "is" : "are")} missing.";
                    break;

                case ParseFailure.TooManyArguments:
                    Reason = $"{Command} doesn't take this many arguments.";
                    break;
            }
        }
    }
}
