using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents a <see cref="DefaultArgumentParser.ParseAsync"/> result.
/// </summary>
public class DefaultArgumentParserResult : IArgumentParserResult
{
    /// <inheritdoc/>
    public bool IsSuccessful => Failure == null;

    /// <summary>
    ///     Gets the failure reason of this <see cref="ClassicArgumentParserFailure"/>.
    /// </summary>
    public string? FailureReason
    {
        get
        {
            if (Failure == null)
                return null;

            switch (Failure.Value)
            {
                case DefaultArgumentParserFailure.UnclosedQuotationMark:
                    return "A quotation mark was left unclosed.";

                case DefaultArgumentParserFailure.UnknownOptionName:
                    return $"Unknown option '{(FailureOptionName.Length == 1 ? "-" : "--")}{FailureOptionName.Span}'.";

                case DefaultArgumentParserFailure.DuplicateOptionName:
                    return $"Duplicate option '{(FailureOptionName.Length == 1 ? "-" : "--")}{FailureOptionName.Span}' specified.";

                case DefaultArgumentParserFailure.MutuallyExclusiveOption:
                {
                    var parameterNames = FailureParameter!.Command.Parameters.OfType<IOptionParameter>()
                        .Where(x => x.Group != null && x.Group.Equals((FailureParameter as IOptionParameter)!.Group) && x != FailureParameter)
                        .Select(x => $"'{x.Name}'")
                        .ToArray();

                    return $"Option '{(FailureOptionName.Length == 1 ? "-" : "--")}{FailureOptionName.Span}' "
                        + $"is mutually exclusive with the {string.Join(", ", parameterNames)} {(parameterNames.Length == 1 ? "parameters" : "parameter")}.";
                }

                case DefaultArgumentParserFailure.ExpectedOptionValue:
                    return $"A value for the option '{(FailureOptionName.Length == 1 ? "-" : "--")}{FailureOptionName.Span}' was expected.";

                case DefaultArgumentParserFailure.TooManyValues:
                {
                    if (FailureParameter == null)
                        return "Too many values provided.";

                    return $"Too many values provided for the {(FailureParameter is IPositionalParameter ? "positional" : "option")} parameter '{FailureParameter.Name}'.";
                }

                default:
                    throw new InvalidOperationException("Invalid argument parser failure.");
            }
        }
    }

    /// <inheritdoc/>
    public IDictionary<IParameter, object?>? Arguments { get; }

    /// <inheritdoc/>
    public IDictionary<IParameter, MultiString>? RawArguments { get; }

    /// <summary>
    ///     Gets the <see cref="ClassicArgumentParserFailure"/>.
    /// </summary>
    public DefaultArgumentParserFailure? Failure { get; }

    /// <summary>
    ///     Gets the name of the option the parse failed on.
    /// </summary>
    public ReadOnlyMemory<char> FailureOptionName { get; }

    /// <summary>
    ///     Gets the parameter the parse failed on.
    /// </summary>
    /// <remarks>
    ///     Can return <see langword="null"/> depending on the <see cref="Failure"/>.
    /// </remarks>
    public ITextParameter? FailureParameter { get; }

    /// <summary>
    ///     Initialises a new successful <see cref="DefaultArgumentParserResult"/>.
    /// </summary>
    /// <param name="arguments"> The parsed arguments. </param>
    /// <param name="rawArguments"> The parsed raw arguments. </param>
    public DefaultArgumentParserResult(
        IDictionary<IParameter, object?>? arguments,
        IDictionary<IParameter, MultiString>? rawArguments)
    {
        Arguments = arguments;
        RawArguments = rawArguments;
    }

    /// <summary>
    ///     Initialises a new unsuccessful <see cref="ClassicArgumentParserResult"/>.
    /// </summary>
    /// <param name="arguments"> The parsed arguments. </param>
    /// <param name="rawArguments"> The parsed raw arguments. </param>
    /// <param name="failure"> The failure. </param>
    /// <param name="failureParameter"> The parameter the parse failed on. </param>
    /// <param name="failureOptionName"> The name of the option the parse failed on. </param>
    public DefaultArgumentParserResult(
        IDictionary<IParameter, object?>? arguments,
        IDictionary<IParameter, MultiString>? rawArguments,
        DefaultArgumentParserFailure failure,
        ReadOnlyMemory<char> failureOptionName = default,
        ITextParameter? failureParameter = null)
        : this(arguments, rawArguments)
    {
        Guard.IsDefined(failure);

        Failure = failure;
        FailureOptionName = failureOptionName;
        FailureParameter = failureParameter;
    }

    /// <summary>
    ///     Implicitly wraps the result into a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="value"> The result to wrap. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> wrapping the result.
    /// </returns>
    public static implicit operator ValueTask<IArgumentParserResult>(DefaultArgumentParserResult value)
        => new(value);
}
