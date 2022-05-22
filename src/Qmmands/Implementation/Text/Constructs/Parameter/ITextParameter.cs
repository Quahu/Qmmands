namespace Qmmands.Text;

/// <summary>
///     Represents a text-based command parameter.
/// </summary>
public interface ITextParameter : IParameter
{
    /// <inheritdoc cref="IParameter.Command"/>
    new ITextCommand Command { get; }

    ICommand IParameter.Command => Command;
}