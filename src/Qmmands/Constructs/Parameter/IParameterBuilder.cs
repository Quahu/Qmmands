using System;
using System.Collections.Generic;
using System.Reflection;
using Qommon;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a builder type for <see cref="IParameter"/>.
/// </summary>
public interface IParameterBuilder : IMetadata
{
    /// <summary>
    ///     Gets the command of this parameter.
    /// </summary>
    ICommandBuilder Command { get; }

    /// <summary>
    ///     Gets or sets the name of this parameter.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the description of this parameter.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the type of this parameter.
    /// </summary>
    Type ReflectedType { get; }

    /// <summary>
    ///     Gets or sets the default value of this parameter,
    ///     i.e. the value that will be used if this parameter is optional and a value is not specified.
    /// </summary>
    Optional<object?> DefaultValue { get; set; }

    /// <summary>
    ///     Gets or sets the type parser type overriding the default parser of this parameter.
    /// </summary>
    Type? CustomTypeParserType { get; set; }

    /// <summary>
    ///     Gets or sets the execution checks of this parameter.
    /// </summary>
    IList<IParameterCheck> Checks { get; }

    /// <summary>
    ///     Gets or sets the custom attributes of this parameter.
    /// </summary>
    IList<Attribute> CustomAttributes { get; }

    /// <summary>
    ///     Gets or sets the parameter information of this parameter.
    ///     Returns <see langword="null"/> if this parameter was not built from a method parameter.
    /// </summary>
    ParameterInfo? ParameterInfo { get; }

    IParameter Build(ICommand command);
}
