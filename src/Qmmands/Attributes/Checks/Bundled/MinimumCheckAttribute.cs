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
            : base(ReflectionUtilities.NumericTypes.Add(typeof(string)))
        {
            Minimum = minimum;
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
            var result = value >= Minimum
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The provided argument must have a minimum {(isString ? "length" : "value")} of {Minimum}.");
#if NETCOREAPP
            return result;
#else
            return Task.FromResult(result);
#endif
        }
    }
}
