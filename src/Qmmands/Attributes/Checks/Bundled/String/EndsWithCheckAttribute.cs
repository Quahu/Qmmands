using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided string argument ends with the provided string value.
    /// </summary>
    public sealed class EndsWithAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the required string value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Gets the <see cref="System.StringComparison"/> used for comparison.
        /// </summary>
        public StringComparison StringComparison { get; }

        /// <summary>
        ///     Initialises a new <see cref="EndsWithAttribute"/> with the specified string value and <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"> The string value. </param>
        public EndsWithAttribute(string value)
            : this(value, StringComparison.OrdinalIgnoreCase)
        { }

        /// <summary>
        ///     Initialises a new <see cref="EndsWithAttribute"/> with the specified string value and <see cref="System.StringComparison"/>.
        /// </summary>
        /// <param name="value"> The string value. </param>
        /// <param name="comparison"> The <see cref="System.StringComparison"/> used for comparison. </param>
        public EndsWithAttribute(string value, StringComparison comparison)
            : base(Utilities.IsStringType)
        {
            Value = value;
            StringComparison = comparison;
        }

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
            => (argument as string).EndsWith(Value, StringComparison)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(
                    $"The provided argument must end with the {(StringComparison.IsCaseSensitive() ? "case-sensitive" : "case-insensitive")} value: {Value}.");
    }
}
