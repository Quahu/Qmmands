using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type that checks the value of a parameter and validates whether the execution of the command can proceed.
/// </summary>
public interface IParameterCheck<in T> : IParameterCheck
{
    /// <inheritdoc/>
    bool IParameterCheck.CanCheck(IParameter parameter, object? value)
        => value is T;

    /// <inheritdoc cref="IParameterCheck.CheckAsync"/>
    ValueTask<IResult> CheckAsync(ICommandContext context, IParameter parameter, T value);

    ValueTask<IResult> IParameterCheck.CheckAsync(ICommandContext context, IParameter parameter, object? value)
        => CheckAsync(context, parameter, (T) value!);
}
