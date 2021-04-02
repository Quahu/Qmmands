﻿using System;
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
        ///     Gets the <see cref="System.StringComparison"/> used for comparison.
        /// </summary>
        public StringComparison StringComparison { get; }

        /// <summary>
        ///     Initialises a new <see cref="ContainsAttribute"/> with the specified string value and <see cref="System.StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"> The string value. </param>
        public ContainsAttribute(string value)
            : this(value, StringComparison.OrdinalIgnoreCase)
        { }

        /// <summary>
        ///     Initialises a new <see cref="ContainsAttribute"/> with the specified string value and <see cref="System.StringComparison"/>.
        /// </summary>
        /// <param name="value"> The string value. </param>
        /// <param name="comparison"> The <see cref="System.StringComparison"/> used for comparison. </param>
        public ContainsAttribute(string value, StringComparison comparison)
        {
            Value = value;
            StringComparison = comparison;
        }

        /// <inheritdoc/>
        public override bool CheckType(Type type)
            => Utilities.IsArrayString(type);

        /// <inheritdoc />
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            if (!(argument is string[] array
                ? Array.Exists(array, x => x.Contains(Value, StringComparison))
                : (argument as string).Contains(Value, StringComparison)))
                return Failure($"The provided {(Parameter.IsMultiple ? "arguments" : "argument")} must contain the {(StringComparison.IsCaseSensitive() ? "case-sensitive" : "case-insensitive")} value: '{Value}'.");

            return Success();
        }
    }
}
