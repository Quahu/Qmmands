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
        {
            Minimum = minimum;
        }

        /// <inheritdoc/>
        public override bool CheckType(Type type)
            => Utilities.IsArrayNullableConvertible(type);

        /// <inheritdoc/>
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            if (argument != null)
            {
                var value = Utilities.ToCheckDouble(argument, out var isString, out var isArray);
                if (value < Minimum)
                    return Failure($"The provided argument{(isArray ? " amount" : isString ? "'s length" : "'s value")} must be a minimum of {Minimum}.");
            }

            return Success();
        }
    }
}
