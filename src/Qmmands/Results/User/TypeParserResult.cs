using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="TypeParser{T}"/> result.
    /// </summary>
    /// <typeparam name="T"> The type handled by the type parser. </typeparam>
    public class TypeParserResult<T> : IResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public bool IsSuccessful => Reason == null;

        /// <summary>
        ///     Gets the error reason. <see langword="null"/> if <see cref="IsSuccessful"/> is <see langword="true"/>.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        ///     Gets whether this result has a parsed value or not.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        ///     Gets the parsed value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified parsed value.
        /// </summary>
        /// <param name="value"> The parsed value. </param>
        public TypeParserResult(T value)
        {
            HasValue = true;
            Value = value;
        }

        /// <summary>
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified error reason.
        /// </summary>
        /// <param name="reason"> The error reason. </param>
        public TypeParserResult(string reason)
        {
            Reason = reason;
        }

        internal TypeParserResult(bool hasValue)
        {
            HasValue = hasValue;
        }

        /// <summary>
        ///     Initialises a new successful <see cref="TypeParserResult{T}"/> with the specified parsed value.
        /// </summary>
        /// <param name="value"> The parsed value. </param>
        /// <returns>
        ///     A successful <see cref="TypeParserResult{T}"/>.
        /// </returns>
        public static TypeParserResult<T> Successful(T value)
            => new TypeParserResult<T>(value);

        /// <summary>
        ///     Initialises a new unsuccessful <see cref="TypeParserResult{T}"/> with the specified error reason.
        /// </summary>
        /// <param name="reason"> The error reason. </param>
        /// <returns>
        ///     An unsuccessful <see cref="TypeParserResult{T}"/>.
        /// </returns>
        public static TypeParserResult<T> Unsuccessful(string reason)
            => new TypeParserResult<T>(reason);

        /// <summary>
        ///     Implicitly wraps the provided <see cref="TypeParserResult{T}"/> in a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result"> The result to wrap. </param>
        public static implicit operator ValueTask<TypeParserResult<T>>(TypeParserResult<T> result)
            => new ValueTask<TypeParserResult<T>>(result);
    }
}
