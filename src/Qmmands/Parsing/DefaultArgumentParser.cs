using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Qmmands
{
    /// <summary>
    ///     The default argument parser used by the <see cref="CommandService"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DefaultArgumentParser : IArgumentParser
    {
        /// <summary>
        ///     Attempts to parse raw arguments for the specified <see cref="Command"/>.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to parse raw arguments for. </param>
        /// <param name="rawArguments"> The raw arguments. </param>
        /// <returns>
        ///     An <see cref="ArgumentParserResult"/>.
        /// </returns>
        public ArgumentParserResult ParseRawArguments(Command command, string rawArguments)
        {
            Parameter currentParameter = null;
            Parameter multipleParameter = null;
            var argumentBuilder = new StringBuilder();
            var arguments = new Dictionary<Parameter, object>();
            var currentQuote = '\0';
            var expectedQuote = '\0';
            var whitespaceSeparated = false;
            var isEscaping = false;

            void NextParameter()
            {
                if (!currentParameter.IsMultiple)
                    arguments.Add(currentParameter, argumentBuilder.ToString());

                else
                {
                    if (arguments.TryGetValue(currentParameter, out var list))
                        (list as List<string>).Add(argumentBuilder.ToString());

                    else
                        arguments[currentParameter] = new List<string> { argumentBuilder.ToString() };
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
                        return new ArgumentParserResult(command, null, rawArguments, arguments, command.Service.QuotationMarkMap.TryGetValue(character, out expectedQuote) && rawArguments.IndexOf(expectedQuote, currentPosition + 1) == -1 ? ArgumentParserFailure.UnexpectedQuote : ArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);

                    else
                    {
                        currentParameter = arguments.Count < command.Parameters.Count && command.Parameters.Count > 0 ? command.Parameters.ElementAt(arguments.Count) : multipleParameter;
                        if (currentParameter == null)
                        {
                            if (command.IgnoreExtraArguments)
                                break;

                            else
                                return new ArgumentParserResult(command, null, rawArguments, arguments, ArgumentParserFailure.TooManyArguments, currentPosition);
                        }

                        else if (currentParameter.IsMultiple)
                            multipleParameter = currentParameter;
                    }
                }

                if (currentParameter.IsRemainder)
                {
                    argumentBuilder.Append(rawArguments.Substring(currentPosition));
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
                            return new ArgumentParserResult(command, currentParameter, rawArguments, arguments, rawArguments.IndexOf(expectedQuote, currentPosition + 1) == -1 ? ArgumentParserFailure.UnexpectedQuote : ArgumentParserFailure.NoWhitespaceBetweenArguments, currentPosition);

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
                return new ArgumentParserResult(command, currentParameter, rawArguments, arguments, ArgumentParserFailure.UnclosedQuote, rawArguments.LastIndexOf(currentQuote));

            if (currentParameter != null)
                NextParameter();

            if (arguments.Count != command.Parameters.Count)
            {
                foreach (var parameter in command.Parameters.Skip(arguments.Count))
                {
                    if (parameter.IsMultiple)
                    {
                        arguments.Add(parameter, new List<string>());
                        break;
                    }

                    if (!parameter.IsOptional)
                        return new ArgumentParserResult(command, parameter, rawArguments, arguments, ArgumentParserFailure.TooFewArguments, null);

                    arguments.Add(parameter, parameter.DefaultValue);
                }
            }

            return new ArgumentParserResult(command, rawArguments, arguments);
        }
    }
}