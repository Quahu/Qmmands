using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a command.
/// </summary>
public interface ICommand : IMetadata
{
    /// <summary>
    ///     Gets the module of this command.
    /// </summary>
    IModule Module { get; }

    /// <summary>
    ///     Gets the parameters of this command.
    /// </summary>
    IReadOnlyList<IParameter> Parameters { get; }

    /// <summary>
    ///     Gets the name of this command.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the description of this command.
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     Gets the execution checks of this command.
    /// </summary>
    IReadOnlyList<ICheck> Checks { get; }

    /// <summary>
    ///     Gets the custom attributes of this command.
    /// </summary>
    IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets the method information of this command.
    ///     Returns <see langword="null"/> if this command was not built from a method.
    /// </summary>
    MethodInfo? MethodInfo { get; }

    /// <summary>
    ///     Gets the callback of this command.
    /// </summary>
    ICommandCallback Callback { get; }
}
