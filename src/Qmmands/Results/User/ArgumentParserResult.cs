using System.Collections.Generic;
using System.Threading.Tasks;
using Qommon.Collections;

namespace Qmmands
{
    /// <summary>
    ///     The base interface for <see cref="IArgumentParser.ParseAsync"/> results.
    /// </summary>
    public abstract class ArgumentParserResult : IResult
    {
        /// <summary>
        ///     Gets whether the result was successful or not.
        /// </summary>
        public abstract bool IsSuccessful { get; }

        /// <summary>
        ///     Gets the failure reason of this <see cref="ArgumentParserResult"/>.
        /// </summary>
        public abstract string Reason { get; }

        /// <summary>
        ///     Gets the successfully parsed arguments.
        /// </summary>
        public IReadOnlyDictionary<Parameter, object> Arguments { get; }

        private static readonly IReadOnlyDictionary<Parameter, object> _emptyParameterDictionary =
            new ReadOnlyDictionary<Parameter, object>(new Dictionary<Parameter, object>(0));

        /// <summary>
        ///     Initialises a new <see cref="ArgumentParserResult"/>.
        /// </summary>
        /// <param name="arguments"> The successfully parsed arguments. </param>
        protected ArgumentParserResult(IReadOnlyDictionary<Parameter, object> arguments)
        {
            Arguments = arguments ?? _emptyParameterDictionary;
        }

        /// <summary>
        ///     Implicitly wraps the provided <see cref="ArgumentParserResult"/> in a <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="result"> The result to wrap. </param>
        public static implicit operator ValueTask<ArgumentParserResult>(ArgumentParserResult result)
            => new ValueTask<ArgumentParserResult>(result);
    }
}
