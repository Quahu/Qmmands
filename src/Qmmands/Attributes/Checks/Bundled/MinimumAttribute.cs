using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided numeric/string argument has the minimum value/length.
    /// </summary>
    public sealed class MinimumAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the minimum value required.
        /// </summary>
        public double Minimum { get; }

        /// <summary>
        ///     Initialises a new <see cref="MinimumAttribute"/> with the specified minimum value.
        /// </summary>
        /// <param name="minimum"> The minimum value. </param>
        public MinimumAttribute(double minimum)
            : base(Utilities.IsNullableConvertible)
        {
            Minimum = minimum;
        }

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            if (argument is IConvertible convertible)
            {
                var isString = convertible.GetTypeCode() == TypeCode.String;
                var value = isString
                    ? (argument as string).Length
                    : Convert.ToDouble(argument);

                if (value < Minimum)
                    return Failure($"The provided argument must have a minimum {(isString ? "length" : "value")} of {Minimum}.");
            }

            return Success();
        }
    }
}
