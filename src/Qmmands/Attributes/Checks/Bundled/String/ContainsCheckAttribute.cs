using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided string argument contains the provided string value.
    /// </summary>
    public sealed class ContainsAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the required string value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Gets whether this check is case-sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; }

        /// <summary>
        ///     Initialises a new <see cref="ContainsAttribute"/> with the specified string value and as case-sensitive.
        /// </summary>
        /// <param name="value"> The string value. </param>
        public ContainsAttribute(string value)
            : this(value, true)
        { }

        /// <summary>
        ///     Initialises a new <see cref="ContainsAttribute"/> with the specified string value and case sensitivity.
        /// </summary>
        /// <param name="value"> The string value. </param>
        /// <param name="isCaseSensitive"> Whether this check is case-sensitive. </param>
        public ContainsAttribute(string value, bool isCaseSensitive)
            : base(new[] { typeof(string) })
        {
            Value = value;
            IsCaseSensitive = isCaseSensitive;
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
            var result = (argument as string).IndexOf(Value, IsCaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase) != -1
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The provided argument must contain the {(IsCaseSensitive ? "case-sensitive" : "case-insensitive")} value: {Value}.");
#if NETCOREAPP
            return result;
#else
            return Task.FromResult(result);
#endif
        }
    }
}
