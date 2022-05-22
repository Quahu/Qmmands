namespace Qmmands.Text;

/// <summary>
///     Represents a builder for type <see cref="IPositionalParameter"/>.
/// </summary>
public interface IPositionalParameterBuilder : ITextParameterBuilder
{
    /// <summary>
    ///     Gets or sets whether this parameter is a remainder parameter,
    ///     i.e. whether the argument parser should pass it all of the remaining input.
    /// </summary>
    bool IsRemainder { get; set; }
}
