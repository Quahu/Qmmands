using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type that checks the value of a parameter and validates whether the execution of the command can proceed.
/// </summary>
public interface IParameterCheck
{
    /// <summary>
    ///     Gets whether this check should be called once for collections
    ///     as a whole or for each element in the collection.
    /// </summary>
    bool ChecksCollection { get; }

    /// <summary>
    ///     Checks whether the given type can be checked by this check.
    /// </summary>
    /// <param name="parameter"> The parameter the value is for. </param>
    /// <param name="value"> The value to check. </param>
    /// <returns>
    ///     <see langword="true"/> if the type can be checked.
    /// </returns>
    bool CanCheck(IParameter parameter, object? value);

    /// <summary>
    ///     Checks whether the parameter value is valid for execution.
    /// </summary>
    /// <param name="context"> The command context. </param>
    /// <param name="parameter"> The parameter. </param>
    /// <param name="value"> The value for the parameter. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the check operation with the result being an <see cref="IResult"/>.
    /// </returns>
    ValueTask<IResult> CheckAsync(ICommandContext context, IParameter parameter, object? value);
}
