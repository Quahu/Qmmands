using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Qommon;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a command parameter.
/// </summary>
public interface IParameter : IMetadata
{
    /// <summary>
    ///     Gets the command of this parameter.
    /// </summary>
    ICommand Command { get; }

    /// <summary>
    ///     Gets the name of this parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the description of this parameter.
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     Gets the original type of this parameter.
    /// </summary>
    Type ReflectedType { get; }

    /// <summary>
    ///     Gets the default value of this parameter,
    ///     i.e. the value that will be used if this parameter is optional and a value is not specified.
    /// </summary>
    Optional<object?> DefaultValue { get; }

    /// <summary>
    ///     Gets the type parser type overriding the default parser of this parameter.
    /// </summary>
    Type? CustomTypeParserType { get; }

    /// <summary>
    ///     Gets the execution checks of this parameter.
    /// </summary>
    IReadOnlyList<IParameterCheck> Checks { get; }

    /// <summary>
    ///     Gets the custom attributes of this parameter.
    /// </summary>
    IReadOnlyList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets the parameter information of this parameter.
    ///     Returns <see langword="null"/> if this parameter was not built from a method parameter.
    /// </summary>
    ParameterInfo? ParameterInfo { get; }
}
