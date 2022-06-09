using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Qommon;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents the original Qmmands argument parser implementation.
/// </summary>
/// <remarks>
///     Supports only positional parameters.
/// </remarks>
public class ClassicArgumentParser : IArgumentParser
{
    public bool SupportsOptionalParameters => false;

    /// <summary>
    ///     Gets the quotation mark pairs used by this parser.
    /// </summary>
    public IDictionary<char, char> QuotationMarks { get; }

    /// <summary>
    ///     Instantiates a new <see cref="ClassicArgumentParser"/>.
    /// </summary>
    /// <param name="options"> The configuration options. </param>
    public ClassicArgumentParser(
        IOptions<DefaultArgumentParserConfiguration> options)
    {
        var configuration = options.Value;
        QuotationMarks = configuration.QuotationMarks;
    }

    /// <inheritdoc/>
    public void Validate(ITextCommand command)
    {
        var parameters = command.Parameters;
        var parameterCount = parameters.Count;
        for (var i = 0; i < parameterCount; i++)
        {
            var parameter = parameters[i];
            if (parameter is not IPositionalParameter)
                Throw.ArgumentException($"The command {command.Name} can not be parsed by the classic argument parser because it contains non-positional parameters.", nameof(command));
        }
    }

    /// <inheritdoc/>
    public ValueTask<IArgumentParserResult> ParseAsync(ITextCommandContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        Guard.IsNotNull(context.Command);

        var command = context.Command;
        var rawArguments = context.RawArgumentString.Span;
        IPositionalParameter? currentParameter = null;
        IPositionalParameter? enumerableParameter = null;
        var argumentBuilder = new StringBuilder();
        Dictionary<IParameter, MultiString>? arguments = null;
        var currentQuote = '\0';
        var expectedQuote = '\0';
        var whitespaceSeparated = false;
        var isEscaping = false;

        // Enumerates the characters in the argument string.
        for (var currentPosition = 0; currentPosition < rawArguments.Length; currentPosition++)
        {
            char character;
            if (currentParameter == null)
            {
                character = rawArguments[currentPosition];
                if (char.IsWhiteSpace(character))
                {
                    // Ensures there's whitespace between arguments, i.e. "abc""def" is invalid.
                    whitespaceSeparated = true;
                    continue;
                }

                if (currentPosition != 0 && !whitespaceSeparated)
                {
                    var failure = QuotationMarks.TryGetValue(character, out expectedQuote) && rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote) == -1
                        ? ClassicArgumentParserFailure.UnexpectedQuotationMark
                        : ClassicArgumentParserFailure.NoWhitespaceBetweenArguments;

                    return new ClassicArgumentParserResult(arguments, null, failure, currentPosition);
                }

                currentParameter = (IPositionalParameter?) (command.Parameters.Count > 0 && (arguments == null || arguments.Count < command.Parameters.Count)
                    ? command.Parameters[arguments?.Count ?? 0]
                    : enumerableParameter);

                if (currentParameter == null)
                {
                    if (command.IgnoresExtraArguments)
                        break;

                    return new ClassicArgumentParserResult(arguments, null, ClassicArgumentParserFailure.TooManyArguments, currentPosition);
                }

                if (enumerableParameter == null && currentParameter.GetTypeInformation().IsEnumerable)
                {
                    enumerableParameter = currentParameter;
                }
            }

            if (currentParameter.IsRemainder)
            {
                argumentBuilder.Append(rawArguments[currentPosition..]);
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

                if (character == '\\' && currentPosition + 1 < rawArguments.Length && QuotationMarks.ContainsKey(rawArguments[currentPosition + 1]))
                {
                    isEscaping = true;
                    continue;
                }

                if (QuotationMarks.TryGetValue(character, out expectedQuote))
                {
                    if (currentPosition != 0 && !whitespaceSeparated)
                    {
                        var failure = rawArguments.Slice(currentPosition + 1).IndexOf(expectedQuote) == -1
                            ? ClassicArgumentParserFailure.UnexpectedQuotationMark
                            : ClassicArgumentParserFailure.NoWhitespaceBetweenArguments;

                        return new ClassicArgumentParserResult(arguments, currentParameter, failure, currentPosition);
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
            return new ClassicArgumentParserResult(arguments, currentParameter, ClassicArgumentParserFailure.UnclosedQuotationMark, rawArguments.LastIndexOf(currentQuote));

        if (currentParameter != null)
            NextParameter(command, ref currentParameter, argumentBuilder, ref arguments);

        if (arguments == null && command.Parameters.Count > 0)
            arguments = new Dictionary<IParameter, MultiString>(command.Parameters.Count);

        if (arguments != null && arguments.Count != command.Parameters.Count)
        {
            for (var i = arguments.Count; i < command.Parameters.Count; i++)
            {
                var parameter = (IPositionalParameter) command.Parameters[i];
                if (!parameter.GetTypeInformation().IsOptional)
                    return new ClassicArgumentParserResult(arguments, parameter, ClassicArgumentParserFailure.TooFewArguments, null);
            }
        }

        return new ClassicArgumentParserResult(arguments);
    }

    private static void NextParameter(ITextCommand command, ref IPositionalParameter currentParameter, StringBuilder argumentBuilder,
        ref Dictionary<IParameter, MultiString>? arguments)
    {
        arguments ??= new Dictionary<IParameter, MultiString>(command.Parameters.Count);

        var argument = argumentBuilder.ToString();
        argumentBuilder.Clear();

        if (currentParameter.GetTypeInformation().IsEnumerable)
        {
            // The parameter takes in multiple values.
            if (arguments.TryGetValue(currentParameter, out var multiString))
            {
                // Add to the existing list.
                (multiString as IList<ReadOnlyMemory<char>>).Add(argument.AsMemory());
            }
            else
            {
                // Create a new list with the argument.
                arguments.Add(currentParameter, MultiString.CreateList(out var list));
                list.Add(argument.AsMemory());
            }
        }
        else
        {
            arguments.Add(currentParameter, argument);
        }

        currentParameter = null!;
    }
}
