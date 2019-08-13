using System;
using System.Collections.Generic;
using System.Linq;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="DefaultArgumentParser.Parse"/> result.
    /// </summary>
    public sealed class DefaultArgumentParserResult : ArgumentParserResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public override bool IsSuccessful => Failure == null;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/>.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed on.
        ///     Can be <see langword="null"/> depending on the <see cref="Failure"/>.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the <see cref="DefaultArgumentParserFailure"/>.
        /// </summary>
        public DefaultArgumentParserFailure? Failure { get; }

        /// <summary>
        ///     Gets the position (index) at which the parsing failed, can be <see langword="null"/> depending on the <see cref="Qmmands.DefaultArgumentParserFailure"/>. 
        /// </summary>
        public int? FailurePosition { get; }

        /// <summary>
        ///     Initialises a new unsuccessful <see cref="DefaultArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Qmmands.Command"/> the parse failed for. </param>
        /// <param name="parameter"> The <see cref="Qmmands.Parameter"/> the parse failed for. </param>
        /// <param name="arguments"> The successfully parsed arguments. </param>
        /// <param name="failure"> The <see cref="DefaultArgumentParserFailure"/>. </param>
        /// <param name="failurePosition"> The failure position. </param>
        public DefaultArgumentParserResult(Command command, Parameter parameter,
            IReadOnlyDictionary<Parameter, object> arguments, DefaultArgumentParserFailure failure, int? failurePosition) : this(command, arguments)
        {
            if (!Enum.IsDefined(typeof(DefaultArgumentParserFailure), failure))
                throw new ArgumentOutOfRangeException(nameof(failure));

            Parameter = parameter;
            Failure = failure;
            FailurePosition = failurePosition;
        }

        /// <summary>
        ///     Initialises a new successful <see cref="DefaultArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Qmmands.Command"/> the parse failed for. </param>
        /// <param name="arguments"> The successfully parsed arguments. </param>
        public DefaultArgumentParserResult(Command command, IReadOnlyDictionary<Parameter, object> arguments) : base(arguments)
        {
            Command = command;
        }

        /// <summary>
        ///     Returns the failure reason of this <see cref="ArgumentParserResult"/>.
        /// </summary>
        public override string GetFailureReason()
        {
            switch (Failure)
            {
                case DefaultArgumentParserFailure.UnclosedQuote:
                    return "A quotation mark was left unclosed.";

                case DefaultArgumentParserFailure.UnexpectedQuote:
                    return "Encountered an unexpected quotation mark.";

                case DefaultArgumentParserFailure.NoWhitespaceBetweenArguments:
                    return "Whitespace is required between arguments.";

                case DefaultArgumentParserFailure.TooFewArguments:
                    var missingParameters = Command.Parameters.SkipWhile(x => x != Parameter).Where(x => !x.IsOptional).Select(x => $"'{x}'").ToArray();
                    return $"Required {(missingParameters.Length == 1 ? "parameter" : "parameters")} " +
                             $"{string.Join(", ", missingParameters)} {(missingParameters.Length == 1 ? "is" : "are")} missing.";

                case DefaultArgumentParserFailure.TooManyArguments:
                    return "Too many arguments provided.";

                default:
                    throw new InvalidOperationException("Invalid argument parser failure.");
            }
        }
    }
}