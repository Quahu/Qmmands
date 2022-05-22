using System;
using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a text-based command.
/// </summary>
public interface ITextCommand : ICommand
{
    /// <inheritdoc cref="ICommand.Module"/>
    new ITextModule Module { get; }

    IModule ICommand.Module => Module;

    /// <inheritdoc cref="ICommand.Parameters"/>
    new IReadOnlyList<ITextParameter> Parameters { get; }

    IReadOnlyList<IParameter> ICommand.Parameters => Parameters;

    /// <summary>
    ///     Gets the aliases of this command.
    /// </summary>
    IReadOnlyList<string> Aliases { get; }

    /// <summary>
    ///     Gets the overload priority of this command.
    /// </summary>
    /// <remarks>
    ///     Higher value means higher priority.
    ///     This is only used when determining the execution order of overloaded commands.
    /// </remarks>
    int OverloadPriority { get; }

    /// <summary>
    ///     Gets whether this command ignores extra arguments.
    /// </summary>
    bool IgnoresExtraArguments { get; }

    /// <summary>
    ///     Gets the argument parser type overriding the default of this command.
    /// </summary>
    Type? CustomArgumentParserType { get; }
}
