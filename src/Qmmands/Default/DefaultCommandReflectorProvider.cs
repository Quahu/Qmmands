using System.Collections.Generic;
using System.Reflection;

namespace Qmmands.Default;

public class DefaultCommandReflectorProvider : ICommandReflectorProvider
{
    private readonly IList<ICommandReflector> _reflectors;

    public DefaultCommandReflectorProvider()
    {
        _reflectors = new List<ICommandReflector>();
    }

    public void AddReflector(ICommandReflector reflector)
    {
        _reflectors.Add(reflector);
    }

    public ICommandReflector? GetReflector(TypeInfo typeInfo)
    {
        if (typeof(IModuleBase).IsAssignableFrom(typeInfo))
        {
            var interfaces = typeInfo.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != typeof(IModuleBase<>))
                    continue;

                var contextType = @interface.GenericTypeArguments[0];
                var reflectors = _reflectors;
                var reflectorCount = reflectors.Count;
                for (var i = reflectorCount - 1; i >= 0; i--)
                {
                    var reflector = reflectors[i];
                    if (reflector.ContextType.IsAssignableFrom(contextType))
                        return reflector;
                }
            }
        }

        return null;
    }
}
