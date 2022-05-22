using System;
using Qommon.Collections.Proxied;

namespace Qmmands.Default;

public class DefaultCommandMapProvider : ProxiedList<ICommandMap>, ICommandMapProvider
{
    public DefaultCommandMapProvider()
    { }

    public ICommandMap? GetMapForModuleType(Type moduleType)
    {
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            var map = this[i];
            if (map.CanMap(moduleType))
                return map;
        }

        return null;
    }

    public ICommandMap? GetMap(Type mapType)
    {
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            var map = this[i];
            if (mapType.IsInstanceOfType(map))
                return map;
        }

        return null;
    }
}
