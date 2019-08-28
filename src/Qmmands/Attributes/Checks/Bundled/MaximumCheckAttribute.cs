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
            : base(Utilities.IsNumericOrStringType)
        {
            Maximum = maximum;
        }

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            var isString = argument is string;
            var value = isString
                ? (argument as string).Length
                : Convert.ToDouble(argument);
            return value <= Maximum
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The provided argument must have a maximum {(isString ? "length" : "value")} of {Maximum}.");
        }
    }
}
