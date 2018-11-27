using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Qmmands
{
    /// <summary>
    ///     Represents an argument parse failure.
    /// </summary>
    public sealed class ArgumentParseFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse failed for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed on, can be null depending on the <see cref="ArgumentParserFailure"/>.
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
            ArgumentParserFailure = argumentParseResult.ArgumentParserFailure != null 
                ? argumentParseResult.ArgumentParserFailure.Value 
                : throw new ArgumentException("Argument parser failure mustn't be null.", nameof(argumentParseResult));
            Position = argumentParseResult.FailurePosition;
            switch (argumentParseResult.ArgumentParserFailure)
            {
                case ArgumentParserFailure.UnclosedQuote:
                    Reason = "A quotation mark was left unclosed.";
                    break;

                case ArgumentParserFailure.UnexpectedQuote:
                    Reason = "Encountered an unexpected quotation mark.";
                    break;

                case ArgumentParserFailure.NoWhitespaceBetweenArguments:
                    Reason = "Whitespace is required between arguments.";
                    break;

                case ArgumentParserFailure.TooFewArguments:
                    var missingParameters = Command.Parameters.SkipWhile(x => x != Parameter).Where(x => !x.IsOptional).Select(x => $"'{x}'").ToImmutableArray();
                    Reason = $"Required {(missingParameters.Length == 1 ? "parameter" : "parameters")} " +
                             $"{string.Join(", ", missingParameters)} {(missingParameters.Length == 1 ? "is" : "are")} missing.";
                    break;

                case ArgumentParserFailure.TooManyArguments:
                    Reason = "Too many arguments provided.";
                    break;
            }
        }
    }
}
