namespace Qmmands.Text;

/// <summary>
///     Represents a builder type for <see cref="ITextParameter"/>.
/// </summary>
public interface ITextParameterBuilder : IParameterBuilder
{
    /// <inheritdoc cref="IParameterBuilder.Command"/>
    new ITextCommandBuilder Command { get; }

    ICommandBuilder IParameterBuilder.Command => Command;

    ITextParameter Build(ITextCommand command);

    IParameter IParameterBuilder.Build(ICommand command)
        => Build(command);
}
