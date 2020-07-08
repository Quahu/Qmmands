using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents the default argument parser used by the <see cref="CommandService"/>.
    /// </summary>
    public sealed class DefaultArgumentParser : IArgumentParser
    {
        /// <summary>
        ///     Gets the singleton instance of the <see cref="DefaultArgumentParser"/>.
        /// </summary>
        public static readonly DefaultArgumentParser Instance = new DefaultArgumentParser();

        private DefaultArgumentParser()
        { }

        /// <summary>
        ///     Attempts to parse raw arguments for the given <see cref="CommandContext"/>.
        /// </summary>
        /// <param name="context"> The <see cref="CommandContext"/> to parse raw arguments for. </param>
        /// <returns>
        ///     A <see cref="DefaultArgumentParserResult"/>.
        /// </returns>
        public ValueTask<ArgumentParserResult> ParseAsync(CommandContext context)
        {
            var command = context.Command;
            var rawArguments = context.RawArguments.AsSpan();
            Parameter currentParameter = null;
            Parameter multipleParameter = null;
            var argumentBuilder = new StringBuilder();
            Dictionary<Parameter, object> arguments = null;
            var currentQuote = '\0';
            var expectedQuote = '\0';
            var whitespaceSeparated = false;
            var isEscaping = false;

            for (var currentPosition = 0; currentPosition < rawArguments.Length; currentPosition++)
            {
                char character;
                if (currentParameter == null)
                {
                    character = rawArguments[currentPosition];
                    if (char.IsWhiteSpace(character))
                    {
                        whitespaceSeparated = true;
                        continue;
                    }
                    else if (currentPosition != 0 && !whitespaceSeparated)
                    {
                        return new DefaultArgumentParserResult(command, null, arguments,
                            command.Service.QuotationMarkMap.TryGetValue(character, out expectedQuote)
                            && rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote) == -1
                                ? DefaultArgumentParserFailure.UnexpectedQuote
                                : DefaultArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);
                    }
                    else
                    {
                        currentParameter = (arguments == null || arguments.Count < command.Parameters.Count)
                            && command.Parameters.Count > 0
                                ? command.Parameters[arguments?.Count ?? 0]
                                : multipleParameter;
                        if (currentParameter == null)
                        {
                            if (command.IgnoresExtraArguments)
                                break;
                            else
                                return new DefaultArgumentParserResult(command, null, arguments, DefaultArgumentParserFailure.TooManyArguments, currentPosition);
                        }
                        else if (currentParameter.IsMultiple)
                        {
                            multipleParameter = currentParameter;
                        }
                    }
                }

                if (currentParameter.IsRemainder)
                {
                    argumentBuilder.Append(rawArguments.Slice(currentPosition));
                    break;
                }

                character = rawArguments[currentPosition];
                if (isEscaping)
                {
                    argumentBuilder.Append(character);
                    isEscaping = false;
                    continue;
                }

                if (currentQuote == '\0')
                {
                    if (char.IsWhiteSpace(character))
                    {
                        whitespaceSeparated = true;
                        NextParameter(command, ref currentParameter, argumentBuilder, ref arguments);
                        continue;
                    }

                    if (character == '\\' && currentPosition + 1 < rawArguments.Length && command.Service.QuotationMarkMap.ContainsKey(rawArguments[currentPosition + 1]))
                    {
                        isEscaping = true;
                        continue;
                    }

                    if (command.Service.QuotationMarkMap.TryGetValue(character, out expectedQuote))
                    {
                        if (currentPosition != 0 && !whitespaceSeparated)
                        {
                            return new DefaultArgumentParserResult(command, currentParameter, arguments,
                               rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote) == -1
                                   ? DefaultArgumentParserFailure.UnexpectedQuote
                                   : DefaultArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);
                        }

                        currentQuote = character;
                        whitespaceSeparated = false;
                        continue;
                    }
                }
                else
                {
                    if (character == '\\' && currentPosition + 1 < rawArguments.Length && rawArguments[currentPosition + 1] == expectedQuote)
                    {
                        isEscaping = true;
                        continue;
                    }

                    if (character == expectedQuote)
                    {
                        currentQuote = '\0';
                        expectedQuote = '\0';
                        NextParameter(command, ref currentParameter, argumentBuilder, ref arguments);
                        continue;
                    }
                }

                argumentBuilder.Append(character);
                whitespaceSeparated = false;
            }

            if (currentQuote != '\0')
                return new DefaultArgumentParserResult(command, currentParameter, arguments, DefaultArgumentParserFailure.UnclosedQuote, rawArguments.LastIndexOf(currentQuote));

            if (currentParameter != null)
                NextParameter(command, ref currentParameter, argumentBuilder, ref arguments);

            if (arguments == null && command.Parameters.Count > 0)
                arguments = new Dictionary<Parameter, object>(command.Parameters.Count);

            if (arguments != null && arguments.Count != command.Parameters.Count)
            {
                for (var i = arguments.Count; i < command.Parameters.Count; i++)
                {
                    var parameter = command.Parameters[i];
                    if (!parameter.IsOptional)
                        return new DefaultArgumentParserResult(command, parameter, arguments, DefaultArgumentParserFailure.TooFewArguments, null);
                }
            }

            return new DefaultArgumentParserResult(command, arguments);
        }

        private static void NextParameter(Command command, ref Parameter currentParameter, StringBuilder argumentBuilder,
            ref Dictionary<Parameter, object> arguments)
        {
            if (arguments == null)
                arguments = new Dictionary<Parameter, object>(command.Parameters.Count);

            if (!currentParameter.IsMultiple)
            {
                arguments.Add(currentParameter, argumentBuilder.ToString());
            }
            else
            {
                if (arguments.TryGetValue(currentParameter, out var list))
                    (list as List<object>).Add(argumentBuilder.ToString());
                else
                    arguments.Add(currentParameter, new List<object> { argumentBuilder.ToString() });
            }

            argumentBuilder.Clear();
            currentParameter = null;
        }
    }
}