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
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed on, can be null depending on the <see cref="ArgumentParserFailure"/>.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the raw arguments passed to the <see cref="IArgumentParser.Parse"/>.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the successfully parsed arguments.
        /// </summary>
        public IReadOnlyDictionary<Parameter, object> Arguments { get; set; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.ArgumentParserFailure"/> that occurred.
        /// </summary>
        public ArgumentParserFailure ArgumentParserFailure { get; }

        /// <summary>
        ///     Gets the position (index) at which the parsing failed, can be null depending on the <see cref="ArgumentParserFailure"/>. 
        /// </summary>
        public int? Position { get; }

        internal ArgumentParseFailedResult(Command command, ArgumentParserResult argumentParseResult)
        {
            Command = command;
            Parameter = argumentParseResult.Parameter;
            RawArguments = argumentParseResult.RawArguments;
            Arguments = argumentParseResult.Arguments;
            ArgumentParserFailure = argumentParseResult.ArgumentParserFailure ?? throw new ArgumentException("Argument parser failure must not be null.", nameof(argumentParseResult));
            Position = argumentParseResult.FailurePosition;

            _lazyReason = new Lazy<string>(() =>
            {
                switch (argumentParseResult.ArgumentParserFailure)
                {
                    case ArgumentParserFailure.UnclosedQuote:
                        return "A quotation mark was left unclosed.";

                    case ArgumentParserFailure.UnexpectedQuote:
                        return "Encountered an unexpected quotation mark.";

                    case ArgumentParserFailure.NoWhitespaceBetweenArguments:
                        return "Whitespace is required between arguments.";

                    case ArgumentParserFailure.TooFewArguments:
                        var missingParameters = Command.Parameters.SkipWhile(x => x != Parameter).Where(x => !x.IsOptional).Select(x => $"'{x}'").ToArray();
                        return $"Required {(missingParameters.Length == 1 ? "parameter" : "parameters")} " +
                                 $"{string.Join(", ", missingParameters)} {(missingParameters.Length == 1 ? "is" : "are")} missing.";

                    case ArgumentParserFailure.TooManyArguments:
                        return "Too many arguments provided.";

                    default:
                        throw new InvalidOperationException("Invalid argument parser failure.");
                }
            }, true);
        }
    }
}
