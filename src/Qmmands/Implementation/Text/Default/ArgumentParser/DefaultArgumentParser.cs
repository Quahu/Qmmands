using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Qommon;
using Qommon.Collections;

namespace Qmmands.Text.Default;

/// <summary>
///     Represents the default argument parser implementation.
/// </summary>
public partial class DefaultArgumentParser : IArgumentParser
{
    public bool SupportsOptionalParameters => true;

    public IDictionary<char, char> QuotationMarks { get; }

    public DefaultArgumentParser(
        IOptions<DefaultArgumentParserConfiguration> options)
    {
        var configuration = options.Value;
        QuotationMarks = configuration.QuotationMarks;

        CommandParameterInformation = new();
    }

    public virtual ParameterInformation GetParameterInformation(ITextCommand command)
        => CommandParameterInformation.GetValue(command, command => new(command));

    /// <inheritdoc/>
    public virtual void Validate(ITextCommand command)
    { }

    protected virtual bool NextValue(ref Dictionary<IParameter, MultiString>? rawArguments,
        IParameter parameter, ReadOnlyMemory<char> value,
        bool isEnumerable)
    {
        rawArguments = rawArguments ??= new Dictionary<IParameter, MultiString>();
        ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(rawArguments, parameter, out var exists);
        if (!exists)
        {
            if (isEnumerable)
            {
                reference = MultiString.CreateList(out var list);
                list.Add(value);
            }
            else
            {
                reference = value;
            }
        }
        else if (!isEnumerable)
        {
            return false;
        }
        else
        {
            (reference as IList<ReadOnlyMemory<char>>).Add(value);
        }

        return true;
    }

    protected virtual DefaultArgumentParserResult Parse(ITextCommandContext context)
    {
        var command = context.Command!;
        var parameterInformation = GetParameterInformation(command);
        Dictionary<IParameter, object?>? arguments = null;
        Dictionary<IParameter, MultiString>? rawArguments = null;
        List<IOptionParameter>? groupParameters = null;

        static bool CheckGroup(ref List<IOptionParameter>? groupParameters, IOptionParameter optionParameter)
        {
            if (optionParameter.Group == null)
                return true;

            if (groupParameters == null)
            {
                groupParameters = new();
                groupParameters.Add(optionParameter);
                return true;
            }

            foreach (var parameter in groupParameters)
            {
                if (parameter.Group == null)
                    continue;

                if (optionParameter.Group.Equals(parameter.Group))
                    return false;
            }

            groupParameters.Add(optionParameter);
            return true;
        }

        var remainingPositionalParameters = new List<IPositionalParameter>(parameterInformation.PositionalParameters.Length);
        for (var i = parameterInformation.PositionalParameters.Length - 1; i >= 0; i--)
            remainingPositionalParameters.Add(parameterInformation.PositionalParameters[i]);

        var quotationMarks = QuotationMarks;
        var sliceEnumerator = new SliceEnumerator(quotationMarks, context.RawArgumentString.TrimStart());
        var tokenEnumerator = new TokenEnumerator(sliceEnumerator);
        IOptionParameter? pendingOptionParameter = null;
        ReadOnlyMemory<char> pendingOptionParameterName = default;
        Token token = default;

        while (!token.IsDefault || tokenEnumerator.MoveNext())
        {
            if (token.IsDefault)
                token = tokenEnumerator.Current;

            if (token.Type == TokenType.Value)
            {
                if (sliceEnumerator.Current.IsQuoted && quotationMarks.ContainsKey(token.Text.Span[0]))
                    return new(arguments, rawArguments, DefaultArgumentParserFailure.UnclosedQuotationMark);

                if (!tokenEnumerator.IsTerminated && pendingOptionParameter != null)
                {
                    if (!NextValue(ref rawArguments, pendingOptionParameter, token.Text, pendingOptionParameter.IsGreedy || pendingOptionParameter.GetTypeInformation().IsEnumerable))
                        return new DefaultArgumentParserResult(arguments, rawArguments, DefaultArgumentParserFailure.DuplicateOptionName, pendingOptionParameterName, pendingOptionParameter);

                    if (!pendingOptionParameter.IsGreedy)
                    {
                        pendingOptionParameter = null;
                        pendingOptionParameterName = default;
                    }
                }
                else
                {
                    if (remainingPositionalParameters.Count == 0)
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.TooManyValues);

                    var positionalParameter = remainingPositionalParameters[^1];
                    var isEnumerable = positionalParameter.IsRemainder || positionalParameter.GetTypeInformation().IsEnumerable;
                    if (!isEnumerable)
                        remainingPositionalParameters.RemoveAt(remainingPositionalParameters.Count - 1);

                    if (!NextValue(ref rawArguments, positionalParameter, token.Text, isEnumerable))
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.TooManyValues, default, positionalParameter);
                }
            }
            else
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool IsSwitch(ParameterExtensions.ParameterTypeInformation typeInformation)
                {
                    return typeInformation.IsBoolean
                        && typeInformation.Parameter.DefaultValue.TryGetValue(out var defaultValue)
                        && defaultValue is bool defaultValueBool
                        && defaultValueBool == false;
                }

                if (pendingOptionParameter != null)
                {
                    if (!pendingOptionParameter.IsGreedy)
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.ExpectedOptionValue, pendingOptionParameterName, pendingOptionParameter);

                    pendingOptionParameter = null;
                    pendingOptionParameterName = default;
                }

                var text = token.Text;
                if (token.Type == TokenType.ShortOption)
                {
                    var shortName = text.Span[0];
                    if (!parameterInformation.TryGetOptionParameter(shortName, out var optionParameter))
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.UnknownOptionName, text[..1]);

                    if (!CheckGroup(ref groupParameters, optionParameter))
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.MutuallyExclusiveOption, text[..1], optionParameter);

                    var typeInformation = optionParameter.GetTypeInformation();
                    if (IsSwitch(typeInformation))
                    {
                        if (arguments != null && arguments.ContainsKey(optionParameter))
                            return new(arguments, rawArguments, DefaultArgumentParserFailure.DuplicateOptionName, text[..1], optionParameter);

                        (arguments ??= new()).Add(optionParameter, true);

                        if (text.Length > 1)
                        {
                            token = Token.ShortOption(text[1..]);
                            continue;
                        }
                    }
                    else
                    {
                        pendingOptionParameter = optionParameter;
                        pendingOptionParameterName = text[..1];

                        if (text.Length > 1)
                        {
                            token = Token.Value(text[1..]);
                            continue;
                        }
                    }
                }
                else if (token.Type == TokenType.LongOption)
                {
                    var equalsIndex = text.Span.IndexOf('=');
                    var longName = equalsIndex != -1
                        ? text[..equalsIndex]
                        : text;

                    if (!parameterInformation.TryGetOptionParameter(longName.Span, out var optionParameter))
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.UnknownOptionName, longName);

                    if (!CheckGroup(ref groupParameters, optionParameter))
                        return new(arguments, rawArguments, DefaultArgumentParserFailure.MutuallyExclusiveOption, longName, optionParameter);

                    var typeInformation = optionParameter.GetTypeInformation();
                    if (IsSwitch(typeInformation))
                    {
                        if (arguments != null && arguments.ContainsKey(optionParameter))
                            return new(arguments, rawArguments, DefaultArgumentParserFailure.DuplicateOptionName, longName, optionParameter);

                        (arguments ??= new()).Add(optionParameter, true);
                    }
                    else
                    {
                        pendingOptionParameter = optionParameter;
                        pendingOptionParameterName = longName;

                        if (equalsIndex != -1 && equalsIndex < text.Length - 1)
                        {
                            token = Token.Value(text[(equalsIndex + 1)..]);
                            continue;
                        }
                    }
                }
            }

            token = default;
        }

        if (rawArguments != null)
        {
            static void ValuesToString(Dictionary<IParameter, MultiString> rawArguments, IParameter parameter)
            {
                ref var lastParameterValue = ref CollectionsMarshal.GetValueRefOrNullRef(rawArguments, parameter);
                if (Unsafe.IsNullRef(ref lastParameterValue) || lastParameterValue.Count <= 1)
                    return;

                var enumerator = lastParameterValue.GetEnumerator();
                enumerator.MoveNext();
                var handler = new DefaultInterpolatedStringHandler(0, 0, null, stackalloc char[256]);
                handler.AppendFormatted(enumerator.Current.Span);
                while (enumerator.MoveNext())
                {
                    handler.AppendLiteral(" ");
                    handler.AppendFormatted(enumerator.Current.Span);
                }

                lastParameterValue = handler.ToStringAndClear();
            }

            if (remainingPositionalParameters.Count > 0)
            {
                var lastParameter = remainingPositionalParameters[^1];
                if (lastParameter.IsRemainder)
                {
                    ValuesToString(rawArguments, lastParameter);
                }
            }

            foreach (var optionParameter in parameterInformation.OptionParameters)
            {
                if (!optionParameter.IsGreedy)
                    continue;

                var typeInformation = optionParameter.GetTypeInformation();
                if (typeInformation.IsEnumerable || !typeInformation.IsStringLike)
                    continue;

                ValuesToString(rawArguments, optionParameter);
            }
        }

        return new(arguments, rawArguments);
    }

    /// <inheritdoc/>
    public virtual ValueTask<IArgumentParserResult> ParseAsync(ITextCommandContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var result = Parse(context);
        return new(result);
    }
}
