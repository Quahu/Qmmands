using System.Reflection;

namespace Qmmands;

/// <summary>
///     Represents a type responsible for providing command callbacks for method information.
/// </summary>
public interface ICommandReflectorCallbackProvider
{
    /// <summary>
    ///     Gets a callback for the given method information.
    /// </summary>
    /// <param name="methodInfo"> The method information. </param>
    /// <returns>
    ///     A command callback.
    /// </returns>
    ICommandCallback GetCallback(MethodInfo methodInfo);
}
