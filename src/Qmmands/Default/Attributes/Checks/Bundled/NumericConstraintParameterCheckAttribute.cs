using System;
using System.Globalization;
using System.Threading.Tasks;
using Qmmands.Default;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided numeric/string argument's value/length meets the implemented criteria.
/// </summary>
public abstract class NumericConstraintParameterCheckAttribute : ParameterCheckAttribute
{
    /// <inheritdoc cref="IParameterCheck.ChecksCollection"/>
    public bool ChecksCollection { get; set; }

    /// <inheritdoc/>
    public override bool CanCheck(IParameter parameter, object? value)
    {
        var typeInformation = parameter.GetTypeInformation();
        return typeInformation.IsEnumerable && ChecksCollection || typeInformation.IsStringLike || typeInformation.IsNumeric;
    }

    /// <inheritdoc/>
    public override ValueTask<IResult> CheckAsync(ICommandContext context, IParameter parameter, object? argument)
    {
        if (argument == null)
            return Results.Success;

        var typeInformation = parameter.GetTypeInformation();
        var locale = context.Locale;
        IResult result;
        if (typeInformation.IsEnumerable && ChecksCollection)
        {
            var count = CheckUtilities.GetCount(argument);
            result = CheckValue(locale, count, true, false);
        }
        else if (typeInformation.IsStringLike)
        {
            var length = CheckUtilities.GetLength(argument);
            result = CheckValue(locale, length, false, true);
        }
        else if (typeInformation.IsInteger)
        {
            var integer = CheckUtilities.GetInteger(locale, argument);
            result = CheckValue(locale, integer, false, false);
        }
        else if (typeInformation.IsNumber)
        {
            var number = CheckUtilities.GetNumber(locale, argument);
            result = CheckValue(locale, number, false, false);
        }
        else
        {
            throw new InvalidOperationException("Invalid argument type.");
        }

        return new(result);
    }

    /// <summary>
    ///     Checks the value.
    /// </summary>
    /// <param name="locale"> The locale to use for conversion. </param>
    /// <param name="value"> The value to check. </param>
    /// <param name="isEnumerable"> Whether the value is the collection count. </param>
    /// <param name="isString"> Whether the value is the string length. </param>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    protected abstract IResult CheckValue<T>(CultureInfo locale, T value, bool isEnumerable, bool isString)
        where T : IComparable<T>;
}
