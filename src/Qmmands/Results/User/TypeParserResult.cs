﻿namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="TypeParser{T}"/> result.
    /// </summary>
    /// <typeparam name="T"> The type handled by the type parser. </typeparam>
    public sealed class TypeParserResult<T> : IResult
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
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified error reason.
        /// </summary>
        /// <param name="reason"> The error reason. </param>
        public TypeParserResult(string reason)
            => Reason = reason;

        /// <summary>
        ///     Initialises a new <see cref="TypeParserResult{T}"/> with the specified parsed value.
        /// </summary>
        /// <param name="value"> The parsed value. </param>
        public TypeParserResult(T value)
        {
            HasValue = true;
            Value = value;
        }

        internal TypeParserResult(bool hasValue)
            => HasValue = hasValue;

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

#if NETCOREAPP
        /// <summary>
        ///     Implicitly wraps the provided <see cref="TypeParserResult{T}"/> in <see cref="System.Threading.Tasks.ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator System.Threading.Tasks.ValueTask<TypeParserResult<T>>(TypeParserResult<T> result)
            => new System.Threading.Tasks.ValueTask<TypeParserResult<T>>(result);
#endif
    }
}
