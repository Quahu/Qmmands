using System;

namespace Qmmands;

/// <summary>
///     Represents a type responsible for retrieving maps suitable for the storing specific module types.
/// </summary>
public interface ICommandMapProvider
{
    /// <summary>
    ///     Gets the map for the specified module type.
    /// </summary>
    /// <param name="moduleType"> The type of the module. </param>
    /// <returns>
    ///     The map for the specified type or <see langword="null"/> if no map was found.
    /// </returns>
    ICommandMap? GetMapForModuleType(Type moduleType);

    /// <summary>
    ///     Gets the map for the specified map type.
    /// </summary>
    /// <param name="mapType"> The type of the map. </param>
    /// <returns>
    ///     The map of the specified type or <see langword="null"/> if the map was not found.
    /// </returns>
    ICommandMap? GetMap(Type mapType);
}
