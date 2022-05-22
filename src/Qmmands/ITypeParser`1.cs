using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <inheritdoc/>
/// <typeparam name="T"> The type parsed. </typeparam>
public interface ITypeParser<T> : ITypeParser
{
    Type ITypeParser.ParsedType => typeof(T);

    /// <inheritdoc cref="ITypeParser.ParseAsync"/>
    new ValueTask<ITypeParserResult<T>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value);

    async ValueTask<ITypeParserResult> ITypeParser.ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
        => await ParseAsync(context, parameter, value).ConfigureAwait(false);
}
