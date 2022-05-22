using System;

namespace Qmmands;

/// <summary>
///     Represents a type used for mapping out the command structure.
/// </summary>
public interface ICommandMap
{
    /// <summary>
    ///     Checks modules of the given type can be mapped by this map.
    /// </summary>
    /// <param name="moduleType"> The module type to check. </param>
    /// <returns>
    ///     <see langword="true"/> if the module type can be mapped.
    /// </returns>
    bool CanMap(Type moduleType);

    // IEnumerable<IModule> EnumerateModules

    /// <summary>
    ///     Maps the given module and its commands.
    /// </summary>
    /// <param name="module"> The module to map. </param>
    void MapModule(IModule module);

    /// <summary>
    ///     Unmaps the given module and its commands.
    /// </summary>
    /// <param name="module"> The module to unmap. </param>
    void UnmapModule(IModule module);
}
