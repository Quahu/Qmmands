using System.Collections.Generic;
using Qommon.Collections;

namespace Qmmands
{
    /// <summary>
    ///     Represents an <see cref="IArgumentParser.Parse"/> result.
    /// </summary>
    public readonly struct ArgumentParserResult : IResult
    {
        private static readonly IReadOnlyDictionary<Parameter, object> _emptyParameterDictionary =
            new ReadOnlyDictionary<Parameter, object>(new Dictionary<Parameter, object>(0));

        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public bool IsSuccessful => ArgumentParserFailure == null;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> the parse was for.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Parameter"/> the parse failed on.
        ///     Can be <see langword="null"/> depending on the <see cref="ArgumentParserFailure"/>.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the raw arguments passed to the <see cref="IArgumentParser.Parse"/>.
        /// </summary>
        public string RawArguments { get; }

        /// <summary>
        ///     Gets the successfully parsed arguments.
        /// </summary>
        public IReadOnlyDictionary<Parameter, object> Arguments { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.ArgumentParserFailure"/>.
        /// </summary>
        public ArgumentParserFailure? ArgumentParserFailure { get; }

        /// <summary>
        ///     Gets the position (index) at which the parsing failed, can be <see langword="null"/> depending on the <see cref="Qmmands.ArgumentParserFailure"/>. 
        /// </summary>
        public int? FailurePosition { get; }

        /// <summary>
        ///     Initialises a new unsuccessful <see cref="ArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Qmmands.Command"/> the parse failed for. </param>
        /// <param name="parameter"> The parameter the parse failed for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <param name="arguments"> The parsed arguments. </param>
        /// <param name="parseFailure"> The <see cref="Qmmands.ArgumentParserFailure"/>. </param>
        /// <param name="failurePosition"> The failure position. </param>
        public ArgumentParserResult(Command command, Parameter parameter, string rawArguments, IReadOnlyDictionary<Parameter, object> arguments, ArgumentParserFailure parseFailure, int? failurePosition) : this()
        {
            Command = command;
            Parameter = parameter;
            RawArguments = rawArguments;
            Arguments = arguments ?? _emptyParameterDictionary;
            ArgumentParserFailure = parseFailure;
            FailurePosition = failurePosition;
        }

        /// <summary>
        ///     Initialises a new successful <see cref="ArgumentParserResult"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Qmmands.Command"/> the parse succeeded for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <param name="arguments"> The parsed arguments. </param>
        public ArgumentParserResult(Command command, string rawArguments, IReadOnlyDictionary<Parameter, object> arguments) : this()
        {
            Command = command;
            RawArguments = rawArguments;
            Arguments = arguments ?? _emptyParameterDictionary;
        }
    }
}