using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter check that ensures the provided string argument matches the provided <see cref="System.Text.RegularExpressions.Regex"/> pattern.
    /// </summary>
    public sealed class RegexAttribute : ParameterCheckAttribute
    {
        /// <summary>
        ///     Gets the required <see cref="System.Text.RegularExpressions.Regex"/>.
        /// </summary>
        public Regex Regex { get; }

        /// <summary>
        ///     Initialises a new <see cref="RegexAttribute"/> with the specified <see cref="System.Text.RegularExpressions.Regex"/> pattern.
        /// </summary>
        /// <param name="pattern"> The <see cref="System.Text.RegularExpressions.Regex"/> pattern. </param>
        public RegexAttribute(string pattern)
            : this(pattern, RegexOptions.Compiled)
        { }

        /// <summary>
        ///     Initialises a new <see cref="RegexAttribute"/> with the specified <see cref="System.Text.RegularExpressions.Regex"/> pattern and <see cref="RegexOptions"/>.
        /// </summary>
        /// <param name="pattern"> The <see cref="System.Text.RegularExpressions.Regex"/> pattern. </param>
        /// <param name="options"> The <see cref="RegexOptions"/>. </param>
        public RegexAttribute(string pattern, RegexOptions options)
            : base(Utilities.IsStringType)
        {
            Regex = new Regex(pattern, options);
        }

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
            => Regex.IsMatch(argument as string)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The provided argument must match the regex pattern: {Regex}.");
    }
}
