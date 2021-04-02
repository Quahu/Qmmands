using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided numeric/string argument has the maximum value/length.
    /// </summary>
    public sealed class MaximumAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the maximum value required.
        /// </summary>
        public double Maximum { get; }

        /// <summary>
        ///     Initialises a new <see cref="MaximumAttribute"/> with the specified maximum value.
        /// </summary>
        /// <param name="maximum"> The maximum value. </param>
        public MaximumAttribute(double maximum)
        {
            Maximum = maximum;
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
                if (value > Maximum)
                    return Failure($"The provided argument{(isArray ? " amount" : isString ? "'s length" : "'s value")} must be a maximum of {Maximum}.");
            }

            return Success();
        }
    }
}
