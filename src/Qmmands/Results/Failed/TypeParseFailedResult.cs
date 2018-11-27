namespace Qmmands
{
    /// <summary>
    ///     Represents a type parse failure.
    /// </summary>
    public sealed class TypeParseFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

        /// <summary>
        ///     Gets the <see cref="Parameter"/> the parse failed for.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the value passed to the type parser.
        /// </summary>
        public string Value { get; }

        internal TypeParseFailedResult(Parameter parameter, string value, string reason)
        {
            Parameter = parameter;
            Value = value;
            Reason = reason;
        }
    }
}
