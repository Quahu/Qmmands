using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents the interface for raw argument parsers.
    /// </summary>
    public interface IArgumentParser
    {
        /// <summary>
        ///     Attempts to parse raw arguments for the given <see cref="CommandContext"/>.
        /// </summary>
        /// <param name="context"> The <see cref="CommandContext"/> to parse raw arguments for. </param>
        /// <returns>
        ///     An <see cref="ArgumentParserResult"/>.
        /// </returns>
        ValueTask<ArgumentParserResult> ParseAsync(CommandContext context);
    }
}
