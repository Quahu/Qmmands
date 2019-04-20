using System;
using System.Collections.Generic;
using System.Text;

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
        ///     An <see cref="ArgumentParserResult"/>.
        /// </returns>
        public ArgumentParserResult Parse(CommandContext context)
        {
            var command = context.Command;
#if NETCOREAPP
            var rawArguments = context.RawArguments.AsSpan();
#else
            var rawArguments = context.RawArguments;
#endif
            Parameter currentParameter = null;
            Parameter multipleParameter = null;
            var argumentBuilder = new StringBuilder();
            Dictionary<Parameter, object> arguments = null;
            var currentQuote = '\0';
            var expectedQuote = '\0';
            var whitespaceSeparated = false;
            var isEscaping = false;

            void NextParameter()
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
                        arguments[currentParameter] = new List<object> { argumentBuilder.ToString() };
                }

                argumentBuilder.Clear();
                currentParameter = null;
            }

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
                        return new ArgumentParserResult(command, null, context.RawArguments, arguments, command.Service.QuotationMarkMap.TryGetValue(character, out expectedQuote)
                           &&
#if NETCOREAPP
                            rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote)
#else
                            rawArguments.IndexOf(expectedQuote, currentPosition + 1)
#endif
                            == -1 ? ArgumentParserFailure.UnexpectedQuote : ArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);
                    }
                    else
                    {
                        currentParameter = (arguments == null || arguments.Count < command.Parameters.Count) && command.Parameters.Count > 0
                            ? command.Parameters[arguments?.Count ?? 0]
                            : multipleParameter;
                        if (currentParameter == null)
                        {
                            if (command.IgnoresExtraArguments)
                                break;
                            else
                                return new ArgumentParserResult(command, null, context.RawArguments, arguments, ArgumentParserFailure.TooManyArguments, currentPosition);
                        }
                        else if (currentParameter.IsMultiple)
                        {
                            multipleParameter = currentParameter;
                        }
                    }
                }

                if (currentParameter.IsRemainder)
                {
                    argumentBuilder.Append(
#if NETCOREAPP
                        rawArguments.Slice(currentPosition)
#else
                        rawArguments.Substring(currentPosition)
#endif
                        );
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
                        NextParameter();
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
                            return new ArgumentParserResult(command, currentParameter, context.RawArguments, arguments,
#if NETCOREAPP
                                rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote)
#else
                                rawArguments.IndexOf(expectedQuote, currentPosition + 1)
#endif
                                == -1 ? ArgumentParserFailure.UnexpectedQuote : ArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);

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
                        NextParameter();
                        continue;
                    }
                }

                argumentBuilder.Append(character);
                whitespaceSeparated = false;
            }

            if (currentQuote != '\0')
                return new ArgumentParserResult(command, currentParameter, context.RawArguments, arguments, ArgumentParserFailure.UnclosedQuote, rawArguments.LastIndexOf(currentQuote));

            if (currentParameter != null)
                NextParameter();

            if (arguments == null && command.Parameters.Count > 0)
                arguments = new Dictionary<Parameter, object>(command.Parameters.Count);

            if (arguments != null && arguments.Count != command.Parameters.Count)
            {
                for (var i = arguments.Count; i < command.Parameters.Count; i++)
                {
                    var parameter = command.Parameters[i];
                    if (parameter.IsMultiple)
                    {
                        arguments.Add(parameter, Array.Empty<object>());
                        break;
                    }

                    if (!parameter.IsOptional)
                        return new ArgumentParserResult(command, parameter, context.RawArguments, arguments, ArgumentParserFailure.TooFewArguments, null);

                    arguments.Add(parameter, parameter.DefaultValue);
                }
            }

            return new ArgumentParserResult(command, context.RawArguments, arguments);
        }
    }
}