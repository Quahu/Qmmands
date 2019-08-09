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
        public override string Reason => _lazyReason.Value;
        private readonly Lazy<string> _lazyReason;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> the parse failed for.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the value passed to the type parser.
        /// </summary>
        public string Value { get; }

        internal TypeParseFailedResult(Parameter parameter, string value, string reason = null)
        {
            Parameter = parameter;
            Value = value;

            if (reason != null)
            {
                _lazyReason = new Lazy<string>(reason);
                return;
            }

            _lazyReason = new Lazy<string>(() =>
            {
                var type = Nullable.GetUnderlyingType(parameter.Type);
                var friendlyName = type == null
                    ? CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(parameter.Type, out var name)
                        ? name
                        : parameter.Type.Name
                    : CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(type, out name)
                        ? $"nullable {name}"
                        : $"nullable {type.Name}";
                return $"Failed to parse {friendlyName}.";
            }, true);
        }
    }
}
