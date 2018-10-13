namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="TypeParser{T}"/> result.
    /// </summary>
    /// <typeparam name="T"> The type handled by the type parser. </typeparam>
    public sealed class TypeParserResult<T> : IResult
    {
        /// <inheritdoc />
        public bool IsSuccessful => Error == null;

        /// <summary>
        ///     Gets the error reason. Null if <see cref="IsSuccessful"/> is <see langword="true"/>.
        /// </summary>
        public string Error { get; }

        /// <summary>
        ///     Gets whether this result has a parsed value or not.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        ///     Gets the parsed value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified error reason.
        /// </summary>
        /// <param name="error"> The error reason. </param>
        public TypeParserResult(string error) : this(false)
            => Error = error;

        /// <summary>
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified value.
        /// </summary>
        /// <param name="value"></param>
        public TypeParserResult(T value) : this(true)
        {
            HasValue = true;
            Value = value;
        }

        internal TypeParserResult(bool hasValue)
            => HasValue = hasValue;
    }
}
