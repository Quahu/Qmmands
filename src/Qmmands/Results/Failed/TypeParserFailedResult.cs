namespace Qmmands
{
    /// <summary>
    ///     Represents a type parser failure.
    /// </summary>
    public sealed class TypeParserFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the <see cref="Parameter"/> the parse failed for.
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        ///     Gets the value passed to the type parser.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public override string Reason { get; }

        internal TypeParserFailedResult(Parameter parameter, string value, string reason)
        {
            Parameter = parameter;
            Value = value;
            Reason = reason;
        }
    }
}
