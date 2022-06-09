namespace Qmmands.Text;

/// <inheritdoc cref="IPositionalParameter"/>
public class PositionalParameter : TextParameter, IPositionalParameter
{
    /// <inheritdoc/>
    public bool IsRemainder { get; }

    public PositionalParameter(ITextCommand command, IPositionalParameterBuilder builder)
        : base(command, builder)
    {
        IsRemainder = builder.IsRemainder;
    }
}
