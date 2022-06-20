using System;
using System.Globalization;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided numeric/string argument's value/length is in the given range.
/// </summary>
public class RangeAttribute : NumericConstraintParameterCheckAttribute
{
    /// <summary>
    ///     Gets the minimum allowed value of the range.
    /// </summary>
    public IConvertible Minimum { get; }

    /// <summary>
    ///     Gets the maximum allowed value of the range.
    /// </summary>
    public IConvertible Maximum { get; }

    /// <summary>
    ///     Instantiates a new <see cref="RangeAttribute"/> with the specified range and inclusion rules.
    /// </summary>
    /// <param name="minimum"> The minimum value of the range. </param>
    /// <param name="maximum"> The maximum value of the range. </param>
    public RangeAttribute(object minimum, object maximum)
    {
        var minimumComparable = Guard.IsAssignableToType<IComparable>(minimum);
        var maximumComparable = Guard.IsAssignableToType<IComparable>(maximum);
        if (maximumComparable.CompareTo(minimumComparable) <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "Maximum must not be smaller than nor equal to minimum.");

        Minimum = Guard.IsAssignableToType<IConvertible>(minimum);
        Maximum = Guard.IsAssignableToType<IConvertible>(maximum);

        Guard.IsTrue(Minimum.GetTypeCode() == Maximum.GetTypeCode(), message: "The minimum and maximum must be of the same type.");
    }

    /// <summary>
    ///     Checks the value against the range.
    /// </summary>
    /// <param name="locale"> The locale to use for conversion. </param>
    /// <param name="value"> The value to check. </param>
    /// <param name="isEnumerable"> Whether the value is the collection count. </param>
    /// <param name="isString"> Whether the value is the string length. </param>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    protected override IResult CheckValue<T>(CultureInfo locale, T value, bool isEnumerable, bool isString)
    {
        var minimum = (T) Minimum.ToType(typeof(T), locale);
        var maximum = (T) Maximum.ToType(typeof(T), locale);
        if (value.CompareTo(minimum) >= 0 && value.CompareTo(maximum) <= 0)
            return Results.Success;

        return Results.Failure($"The provided argument{(isEnumerable ? " amount" : isString ? "'s length" : "'s value")} was outside of the range: [{minimum}, {maximum}].");
    }
}
