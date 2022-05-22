using System.Reflection;

namespace Qmmands;

public interface ICommandReflectorProvider
{
    ICommandReflector? GetReflector(TypeInfo typeInfo);
}
