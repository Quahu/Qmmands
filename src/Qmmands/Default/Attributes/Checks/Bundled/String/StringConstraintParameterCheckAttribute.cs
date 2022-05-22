using System;
using System.Globalization;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents a parameter check that ensures the provided numeric/string argument's value/length meets the implemented criteria.
/// </summary>
public abstract class StringConstraintParameterCheckAttribute : ParameterCheckAttribute
{
    /// <inheritdoc/>
    public override bool CanCheck(IParameter parameter, object? value)
    {
        var typeInformation = parameter.GetTypeInformation();
        return typeInformation.IsStringLike;
    }

    /// <inheritdoc/>
    public override ValueTask<IResult> CheckAsync(ICommandContext context, IParameter parameter, object? argument)
    {
        if (argument == null)
            return Results.Success;

        var value = argument switch
        {
            string stringValue => stringValue.AsMemory(),
            ReadOnlyMemory<char> memoryValue => memoryValue,
            MultiString multiString => multiString[0],
            _ => Throw.ArgumentOutOfRangeException<ReadOnlyMemory<char>>(nameof(argument), argument, null)
        };

        var typeInformation = parameter.GetTypeInformation();
        var locale = context.Locale;
        var result = CheckValue(locale, value, typeInformation.IsEnumerable);
        return new(result);
    }

    /// <summary>
    ///     Checks the value.
    /// </summary>
    /// <param name="locale"> The locale to use for conversion. </param>
    /// <param name="value"> The value to check. </param>
    /// <param name="isEnumerable"> Whether the argument is an enumerable. </param>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    protected abstract IResult CheckValue(CultureInfo locale, ReadOnlyMemory<char> value, bool isEnumerable);
}
