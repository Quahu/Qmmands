using System;
using System.Reflection;

namespace Qmmands;

public interface ICommandReflector
{
    Type ContextType { get; }

    IModule CreateModule(TypeInfo typeInfo, Action<IModuleBuilder>? builderAction = null);
}
