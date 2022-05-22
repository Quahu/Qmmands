namespace Qmmands.Text;

/// <summary>
///     Represents a text command management service.
/// </summary>
public interface ITextCommandService : ICommandService
{
    /// <summary>
    ///     Gets the argument parser service.
    /// </summary>
    IArgumentParserProvider ArgumentParsers { get; }
}