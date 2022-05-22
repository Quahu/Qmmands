using System;
using System.Globalization;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided numeric/string argument has the minimum value/length.
/// </summary>
public class MinimumAttribute : NumericConstraintParameterCheckAttribute
{
    /// <summary>
    ///     Gets the minimum allowed value.
    /// </summary>
    public IConvertible Minimum { get; }

    /// <summary>
    ///     Instantiates a new <see cref="MinimumAttribute"/> with the specified minimum value.
    /// </summary>
    /// <param name="minimum"> The minimum value. </param>
    public MinimumAttribute(object minimum)
    {
        Minimum = Guard.IsAssignableToType<IConvertible>(minimum);
    }

    /// <inheritdoc/>
    protected override IResult CheckValue<T>(CultureInfo locale, T value, bool isEnumerable, bool isString)
    {
        var minimum = (T) Minimum.ToType(typeof(T), locale);
        if (value.CompareTo(minimum) >= 0)
            return Results.Success;

        return Results.Failure($"The provided argument{(isEnumerable ? " amount" : isString ? "'s length" : "'s value")} must be a minimum of {minimum}.");
    }
}
