using System;
using System.ComponentModel;

namespace Qmmands;

/// <summary>
///     Represents extensions for <see cref="ICommandMap"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandMapProviderExtensions
{
    /// <summary>
    ///     Gets the map for the specified map type.
    /// </summary>
    /// <param name="provider"> The command map provider. </param>
    /// <param name="moduleType"> The type of the module. </param>
    /// <exception cref="InvalidOperationException">
    ///     No command map of type <paramref name="moduleType"/> found.
    /// </exception>
    /// <returns>
    ///     The map of the specified type.
    /// </returns>
    public static ICommandMap GetRequiredMapForModuleType(this ICommandMapProvider provider, Type moduleType)
        => provider.GetMapForModuleType(moduleType) ?? throw new InvalidOperationException($"No command map for type {moduleType} found.");

    /// <summary>
    ///     Gets the map for the specified map type.
    /// </summary>
    /// <param name="provider"> The command map provider. </param>
    /// <returns>
    ///     The map of the specified type or <see langword="null"/> if the map was not found.
    /// </returns>
    public static TCommandMap? GetMap<TCommandMap>(this ICommandMapProvider provider)
        where TCommandMap : class
        => provider.GetMap(typeof(TCommandMap)) as TCommandMap;

    /// <summary>
    ///     Gets the map for the specified map type.
    /// </summary>
    /// <param name="provider"> The command map provider. </param>
    /// <typeparam name="TCommandMap"> The type of the command map. </typeparam>
    /// <exception cref="InvalidOperationException">
    ///     No command map of type <typeparamref name="TCommandMap"/> found.
    /// </exception>
    /// <returns>
    ///     The map of the specified type.
    /// </returns>
    public static TCommandMap GetRequiredMap<TCommandMap>(this ICommandMapProvider provider)
        where TCommandMap : class
        => provider.GetMap(typeof(TCommandMap)) as TCommandMap ?? throw new InvalidOperationException($"No command map of type {typeof(TCommandMap)} found.");
}
