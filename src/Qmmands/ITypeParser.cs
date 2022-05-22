using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type parser that converts <see cref="string"/> arguments to the desired type.
/// </summary>
public interface ITypeParser
{
    /// <summary>
    ///     Gets the type parsed by this parser.
    /// </summary>
    Type ParsedType { get; }

    /// <summary>
    ///     Attempts to parse the provided <paramref name="value"/> into the desired type.
    /// </summary>
    /// <param name="context"> The command context. </param>
    /// <param name="parameter"> The parameter for which the value is being parsed. </param>
    /// <param name="value"> The value to parse. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the parse operation with the result being an <see cref="ITypeParserResult"/>.
    /// </returns>
    ValueTask<ITypeParserResult> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> value);
}
