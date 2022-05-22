using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qommon;
using Qommon.Collections.ReadOnly;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents a <see cref="ClassicArgumentParser.ParseAsync"/> result.
/// </summary>
[Obsolete(ClassicArgumentParser.ObsoletionReason)]
public class ClassicArgumentParserResult : IArgumentParserResult
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
                case ClassicArgumentParserFailure.UnclosedQuotationMark:
                    return "A quotation mark was left unclosed.";

                case ClassicArgumentParserFailure.UnexpectedQuotationMark:
                    return "Encountered an unexpected quotation mark.";

                case ClassicArgumentParserFailure.NoWhitespaceBetweenArguments:
                    return "Whitespace is required between arguments.";

                case ClassicArgumentParserFailure.TooFewArguments:
                    var missingParameters = EnumerateMissingParameters().Select(parameter => $"'{parameter.Name}'").ToArray();
                    return $"Required {(missingParameters.Length == 1 ? "parameter" : "parameters")} " +
                        $"{string.Join(", ", missingParameters)} {(missingParameters.Length == 1 ? "is" : "are")} missing.";

                case ClassicArgumentParserFailure.TooManyArguments:
                    return "Too many arguments provided.";

                default:
                    throw new InvalidOperationException("Invalid argument parser failure.");
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<ITextParameter, MultiString> Arguments { get; }

    /// <summary>
    ///     Gets the <see cref="ClassicArgumentParserFailure"/>.
    /// </summary>
    public ClassicArgumentParserFailure? Failure { get; }

    /// <summary>
    ///     Gets the <see cref="ITextParameter"/> the parse failed on.
    ///     Can return <see langword="null"/> depending on the <see cref="Failure"/>.
    /// </summary>
    public IPositionalParameter? FailureParameter { get; }

    /// <summary>
    ///     Gets the position (index) at which the parsing failed, can be <see langword="null"/> depending on the <see cref="ClassicArgumentParserFailure"/>.
    /// </summary>
    public int? FailurePosition { get; }

    /// <summary>
    ///     Initialises a new successful <see cref="ClassicArgumentParserResult"/>.
    /// </summary>
    /// <param name="arguments"> The parsed arguments. </param>
    public ClassicArgumentParserResult(
        IReadOnlyDictionary<ITextParameter, MultiString>? arguments)
    {
        Arguments = arguments ?? ReadOnlyDictionary<ITextParameter, MultiString>.Empty;
    }

    /// <summary>
    ///     Initialises a new unsuccessful <see cref="ClassicArgumentParserResult"/>.
    /// </summary>
    /// <param name="arguments"> The parsed arguments. </param>
    /// <param name="failureParameter"> The <see cref="ITextParameter"/> the parse failed for. </param>
    /// <param name="failure"> The <see cref="ClassicArgumentParserFailure"/>. </param>
    /// <param name="failurePosition"> The failure position. </param>
    public ClassicArgumentParserResult(
        IReadOnlyDictionary<ITextParameter, MultiString>? arguments, IPositionalParameter? failureParameter,
        ClassicArgumentParserFailure failure, int? failurePosition)
        : this(arguments)
    {
        Guard.IsDefined(failure);

        FailureParameter = failureParameter;
        Failure = failure;
        FailurePosition = failurePosition;
    }

    /// <summary>
    ///     Enumerates missing parameters.
    /// </summary>
    public IEnumerable<IPositionalParameter> EnumerateMissingParameters()
    {
        var failureParameter = FailureParameter;
        if (failureParameter == null)
            return Array.Empty<IPositionalParameter>();

        return failureParameter.Command.Parameters.Cast<IPositionalParameter>().SkipWhile(parameter => parameter != failureParameter).Where(parameter => !parameter.GetTypeInformation().IsOptional);
    }

    /// <summary>
    ///     Implicitly wraps the result into a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="value"> The result to wrap. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> wrapping the result.
    /// </returns>
    public static implicit operator ValueTask<IArgumentParserResult>(ClassicArgumentParserResult value)
        => new(value);
}
