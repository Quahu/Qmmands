using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a builder type for <see cref="ICommand"/>.
/// </summary>
public interface ICommandBuilder : IMetadata
{
    /// <summary>
    ///     Gets the module of this command.
    /// </summary>
    IModuleBuilder Module { get; }

    /// <summary>
    ///     Gets the parameters of this command.
    /// </summary>
    IList<IParameterBuilder> Parameters { get; }

    /// <summary>
    ///     Gets or sets the name of this command.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the description of this command.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets the execution checks of this command.
    /// </summary>
    IList<ICheck> Checks { get; }

    /// <summary>
    ///     Gets the custom attributes of this command.
    /// </summary>
    IList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets the method information of this command.
    ///     Returns <see langword="null"/> if this command was not built from a method.
    /// </summary>
    MethodInfo? MethodInfo { get; }

    /// <summary>
    ///     Gets the callback of this command.
    /// </summary>
    ICommandCallback Callback { get; }

    ICommand Build(IModule module);
}
