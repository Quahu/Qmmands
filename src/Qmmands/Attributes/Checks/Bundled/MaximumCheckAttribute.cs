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
            : base(ReflectionUtilities.NumericTypes.Add(typeof(string)))
        {
            Maximum = maximum;
        }

        /// <inheritdoc />
        public override
#if NETCOREAPP
            ValueTask<CheckResult>
#else
            Task<CheckResult>
#endif
            CheckAsync(object argument, CommandContext context, IServiceProvider provider)
        {
            var isString = argument is string;
            var value = isString
                ? (argument as string).Length
                : Convert.ToDouble(argument);
            var result = value <= Maximum
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The provided argument must have a maximum {(isString ? "length" : "value")} of {Maximum}.");
#if NETCOREAPP
            return result;
#else
            return Task.FromResult(result);
#endif
        }
    }
}
