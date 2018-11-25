using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     The base class for type parsers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TypeParser<T> : ITypeParser
    {
        /// <summary>
        ///     The overridable method for type parsing logic.
        /// </summary>
        /// <param name="value"> The raw argument to parse. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> used during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used during execution. </param>
        /// <returns> A <see cref="TypeParserResult{T}"/>. </returns>
        public abstract Task<TypeParserResult<T>> ParseAsync(string value, ICommandContext context, IServiceProvider provider);

        async Task<TypeParserResult<object>> ITypeParser.ParseAsync(string value, ICommandContext context, IServiceProvider provider)
        {
            var result = await ParseAsync(value, context, provider).ConfigureAwait(false);
            return result.IsSuccessful
                ? result.HasValue
                    ? new TypeParserResult<object>(result.Value)
                    : new TypeParserResult<object>(false)
                : new TypeParserResult<object>(result.Reason);
        }
    }
}
