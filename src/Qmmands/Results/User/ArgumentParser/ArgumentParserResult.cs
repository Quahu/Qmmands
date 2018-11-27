using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Represents an <see cref="IArgumentParser.ParseRawArguments"/> result.
    /// </summary>
    public struct ArgumentParserResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccessful { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse was for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Parameter"/> the parse failed on, can be <see langword="null"/> depending on the <see cref="Qmmands.ArgumentParserFailure"/>.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the raw arguments passed to the <see cref="IArgumentParser.ParseRawArguments"/>.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the successfully parsed arguments.
        /// </summary>
        public IReadOnlyDictionary<Parameter, object> Arguments { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.ArgumentParserFailure"/>.
        /// </summary>
        public ArgumentParserFailure? ParseFailure { get; }

        /// <summary>
        ///     Gets the position (index) at which the parsing failed, can be <see langword="null"/> depending on the <see cref="Qmmands.ArgumentParserFailure"/>. 
        /// </summary>
        public int? FailurePosition { get; }

        /// <summary>
        ///     Initialises a new failed <see cref="ArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The command the parse failed for. </param>
        /// <param name="parameter"> The parameter the parse failed for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <param name="arguments"> The parsed arguments. </param>
        /// <param name="parseFailure"> The <see cref="Qmmands.ArgumentParserFailure"/>. </param>
        /// <param name="failurePosition"> The failure position. </param>
        public ArgumentParserResult(Command command, Parameter parameter, string rawArguments, IReadOnlyDictionary<Parameter, object> arguments, ArgumentParserFailure parseFailure, int? failurePosition) : this()
        {
            IsSuccessful = false;
            Command = command;
            Parameter = parameter;
            RawArguments = rawArguments;
            Arguments = arguments;
            ParseFailure = parseFailure;
            FailurePosition = failurePosition;
        }

        /// <summary>
        ///     Initialises a new successful <see cref="ArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The command the parse succeeded for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <param name="arguments"> The parsed arguments. </param>
        public ArgumentParserResult(Command command, string rawArguments, IReadOnlyDictionary<Parameter, object> arguments) : this()
        {
            IsSuccessful = true;
            Command = command;
            RawArguments = rawArguments;
            Arguments = arguments;
        }
    }
}