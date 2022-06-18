using System.Reflection;

namespace Qmmands;

/// <inheritdoc/>
public class ReflectionCommandReflectorCallbackProvider : ICommandReflectorCallbackProvider
{
    /// <inheritdoc/>
    public virtual ICommandCallback GetCallback(MethodInfo methodInfo)
    {
        return ReflectionCommandCallback.Instance;
    }
}
