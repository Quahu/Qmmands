using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents a type parse failure.
    /// </summary>
    public sealed class TypeParseFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>
        public override string FailureReason => _reason ?? GetDefaultReason();

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed for.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the value passed to the type parser.
        /// </summary>
        public string Value { get; }

        private readonly string _reason;

        internal TypeParseFailedResult(Parameter parameter, string value, string reason = null)
        {
            Parameter = parameter;
            Value = value;
            _reason = reason;
        }

        private string GetDefaultReason()
        {
            var type = Nullable.GetUnderlyingType(Parameter.Type);
            var friendlyName = type == null
                ? CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(Parameter.Type, out var name)
                    ? name
                    : Parameter.Type.Name
                : CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(type, out name)
                    ? $"nullable {name}"
                    : $"nullable {type.Name}";

            return $"Failed to parse {friendlyName}.";
        }
    }
}
