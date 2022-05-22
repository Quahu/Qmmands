using System.Threading.Tasks;

namespace Qmmands.Text;

/// <summary>
///     Represents an argument parser.
/// </summary>
public interface IArgumentParser
{
    bool SupportsOptionalParameters { get; }

    /// <summary>
    ///     Validates that the command can be parsed by this parser.
    /// </summary>
    /// <param name="command"> The command to validate. </param>
    void Validate(ITextCommand command);

    /// <summary>
    ///     Attempts to parse the <see cref="ITextCommandContext.RawArgumentString"/> into <see cref="ICommandContext.RawArguments"/>.
    /// </summary>
    /// <param name="context"> The command context. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the parse operation with the result being an <see cref="IArgumentParserResult"/>.
    /// </returns>
    ValueTask<IArgumentParserResult> ParseAsync(ITextCommandContext context);
}
