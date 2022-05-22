namespace Qmmands.Text;

public class PositionalParameter : TextParameter, IPositionalParameter
{
    public bool IsRemainder { get; }

    public PositionalParameter(ITextCommand command, IPositionalParameterBuilder builder)
        : base(command, builder)
    {
        IsRemainder = builder.IsRemainder;
    }
}
