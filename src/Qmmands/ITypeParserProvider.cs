namespace Qmmands;

/// <summary>
///     Represents a service responsible for providing and persisting type parser instances.
/// </summary>
public interface ITypeParserProvider
{
    /// <summary>
    ///     Gets a type parser instance for the specified parameter.
    /// </summary>
    /// <param name="parameter"> The parameter to get the parser for. </param>
    /// <returns>
    ///     An <see cref="ITypeParser"/> or <see langword="null"/>.
    /// </returns>
    ITypeParser? GetParser(IParameter parameter);
}
