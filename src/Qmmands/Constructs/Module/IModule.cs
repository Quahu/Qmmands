using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a command module.
/// </summary>
public interface IModule : IMetadata
{
    /// <summary>
    ///     Gets the parent module of this module.
    /// </summary>
    IModule? Parent { get; }

    /// <summary>
    ///     Gets the submodules of this module.
    /// </summary>
    IReadOnlyList<IModule> Submodules { get; }

    /// <summary>
    ///     Gets the commands of this module.
    /// </summary>
    IReadOnlyList<ICommand> Commands { get; }

    /// <summary>
    ///     Gets the name of this module.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the description of this module.
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     Gets the checks of this module.
    /// </summary>
    IReadOnlyList<ICheck> Checks { get; }

    /// <summary>
    ///     Gets the custom attributes of this module.
    /// </summary>
    IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets the type information of this module.
    ///     Returns <see langword="null"/> if this module was not built from a <see langword="class"/>.
    /// </summary>
    TypeInfo? TypeInfo { get; }
}
