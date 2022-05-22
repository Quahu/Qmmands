using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a builder type for <see cref="IModule"/>.
/// </summary>
public interface IModuleBuilder : IMetadata
{
    /// <summary>
    ///     Gets or sets the parent module of this module.
    /// </summary>
    IModuleBuilder? Parent { get; }

    /// <summary>
    ///     Gets or sets the submodules of this module.
    /// </summary>
    IList<IModuleBuilder> Submodules { get; }

    /// <summary>
    ///     Gets or sets the commands of this module.
    /// </summary>
    IList<ICommandBuilder> Commands { get; }

    /// <summary>
    ///     Gets or sets the name of this module.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the description of this module.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the checks of this module.
    /// </summary>
    IList<ICheck> Checks { get; }

    /// <summary>
    ///     Gets or sets the custom attributes of this module.
    /// </summary>
    IList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets the type information of this module.
    ///     Returns <see langword="null"/> if this module was not built from a <see langword="class"/>.
    /// </summary>
    TypeInfo? TypeInfo { get; }

    IModule Build(IModule? parent = null);
}
