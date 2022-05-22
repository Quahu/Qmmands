using System;
using System.Globalization;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided numeric/string argument has the maximum value/length.
/// </summary>
public class MaximumAttribute : NumericConstraintParameterCheckAttribute
{
    /// <summary>
    ///     Gets the maximum allowed value.
    /// </summary>
    public IConvertible Maximum { get; }

    /// <summary>
    ///     Instantiates a new <see cref="MaximumAttribute"/> with the specified maximum value.
    /// </summary>
    /// <param name="maximum"> The maximum value. </param>
    public MaximumAttribute(object maximum)
    {
        Maximum = Guard.IsAssignableToType<IConvertible>(maximum);
    }

    /// <inheritdoc/>
    protected override IResult CheckValue<T>(CultureInfo locale, T value, bool isEnumerable, bool isString)
    {
        var maximum = (T) Maximum.ToType(typeof(T), locale);
        if (value.CompareTo(maximum) <= 0)
            return Results.Success;

        return Results.Failure($"The provided argument{(isEnumerable ? " amount" : isString ? "'s length" : "'s value")} must be a minimum of {maximum}.");
    }
}
