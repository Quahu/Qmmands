using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     The base class for user type parsers.
    /// </summary>
    /// <typeparam name="T"> The <see cref="Type"/> parsed by this <see cref="TypeParser{T}"/>. </typeparam>
    public abstract class TypeParser<T> : ITypeParser
    {
        /// <summary>
        ///     The overridable method for type parsing logic.
        /// </summary>
        /// <param name="parameter"> The currently parsed <see cref="Parameter"/>. </param>
        /// <param name="value"> The raw argument to parse. </param>
        /// <param name="context"> The <see cref="CommandContext"/> used during execution. </param>
        /// <returns> A <see cref="TypeParserResult{T}"/>. </returns>
        public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context);

        async ValueTask<TypeParserResult<object>> ITypeParser.ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var result = await ParseAsync(parameter, value, context).ConfigureAwait(false);
            return result.IsSuccessful
                ? result.HasValue
                    ? new TypeParserResult<object>(result.Value)
                    : new TypeParserResult<object>(false)
                : new TypeParserResult<object>(result.Reason);
        }
    }
}
