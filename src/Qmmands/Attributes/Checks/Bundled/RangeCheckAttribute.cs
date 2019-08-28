using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided numeric/string argument's value/length is in the given range.
    /// </summary>
    public sealed class RangeAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the minimum value of the range.
        /// </summary>
        public double Minimum { get; }

        /// <summary>
        ///     Gets the maximum value of the range.
        /// </summary>
        public double Maximum { get; }

        /// <summary>
        ///     Gets whether the minimum range value is inclusive or not.
        /// </summary>
        public bool IsMinimumInclusive { get; }

        /// <summary>
        ///     Gets whether the maximum range value is inclusive or not.
        /// </summary>
        public bool IsMaximumInclusive { get; }

        /// <summary>
        ///     Initialises a new <see cref="RangeAttribute"/> with the specified range and with the following default values:
        ///     <para> <see cref="IsMinimumInclusive"/> set to <see langword="true"/>. </para>
        ///     <para> <see cref="IsMaximumInclusive"/> set to <see langword="false"/>. </para>
        /// </summary>
        /// <param name="minimum"> The minimum value of the range. </param>
        /// <param name="maximum"> The maximum value of the range. </param>
        public RangeAttribute(double minimum, double maximum)
            : this(minimum, maximum, true, false)
        { }

        /// <summary>
        ///     Initialises a new <see cref="RangeAttribute"/> with the specified range and inclusion rules.
        /// </summary>
        /// <param name="minimum"> The minimum value of the range. </param>
        /// <param name="maximum"> The maximum value of the range. </param>
        /// <param name="isMinimumInclusive"> Whether the minimal value is inclusive or not. </param>
        /// <param name="isMaximumInclusive"> Whether the maximum value is inclusive or not. </param>
        public RangeAttribute(double minimum, double maximum, bool isMinimumInclusive, bool isMaximumInclusive)
            : base(Utilities.IsNumericOrStringType)
        {
            if (maximum < minimum)
                throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "Maximum must not be smaller than minimum.");

            if (maximum == minimum)
                throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "Maximum must not be equal to minimum.");

            Minimum = minimum;
            Maximum = maximum;
            IsMinimumInclusive = isMinimumInclusive;
            IsMaximumInclusive = isMaximumInclusive;
        }

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            var value = (argument as string)?.Length ?? Convert.ToDouble(argument);
            return (IsMinimumInclusive && !IsMaximumInclusive
                ? Minimum <= value && value < Maximum
                : !IsMinimumInclusive && IsMaximumInclusive
                    ? Minimum < value && value <= Maximum
                    : IsMinimumInclusive && IsMaximumInclusive
                        ? Minimum <= value && value <= Maximum
                        : Minimum < value && value < Maximum)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(
                    $"The provided argument was outside of the range: {(IsMinimumInclusive ? '[' : '(')}{Minimum}, {Maximum}{(IsMaximumInclusive ? ']' : ')')}.");
        }
    }
}
