namespace Qmmands.Text;

/// <summary>
///     Represents a service responsible for providing and persisting argument parser instances.
/// </summary>
public interface IArgumentParserProvider
{
    /// <summary>
    ///     Gets an argument parser instance for the specified command.
    /// </summary>
    /// <param name="command"> The command to get the parser for. </param>
    /// <returns>
    ///     An <see cref="IArgumentParser"/> or <see langword="null"/>.
    /// </returns>
    IArgumentParser? GetParser(ITextCommand command);
}
