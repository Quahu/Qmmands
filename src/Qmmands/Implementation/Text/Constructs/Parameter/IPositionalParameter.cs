namespace Qmmands.Text;

/// <summary>
///     Represents a positional parameter, i.e. a parameter that can be specified positionally.
/// </summary>
public interface IPositionalParameter : ITextParameter
{
    /// <summary>
    ///     Gets whether this parameter is a remainder parameter,
    ///     i.e. whether the argument parser should pass it all of the remaining input.
    /// </summary>
    bool IsRemainder { get; }
}