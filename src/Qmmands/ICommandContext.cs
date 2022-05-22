using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Qommon;
using Qommon.Metadata;

namespace Qmmands;

/// <summary>
///     Represents a command execution context.
/// </summary>
public interface ICommandContext : ISynchronizedMetadata
{
    /// <summary>
    ///     Gets or sets the services of this context.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Gets or sets the cancellation token of this context.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    ///     Gets or sets the locale of this context.
    /// </summary>
    CultureInfo Locale { get; }

    /// <summary>
    ///     Gets or sets the current command execution step of this context.
    /// </summary>
    ICommandExecutionStep? ExecutionStep { get; set; }

    /// <summary>
    ///     Gets or sets the executed command.
    /// </summary>
    ICommand? Command { get; set; }

    /// <summary>
    ///     Gets or sets the raw character arguments that should be type parsed.
    /// </summary>
    IDictionary<IParameter, MultiString>? RawArguments { get; set; }

    /// <summary>
    ///     Gets or sets the arguments for the command's parameters.
    /// </summary>
    IDictionary<IParameter, object?>? Arguments { get; set; }

    /// <summary>
    ///     Gets or sets the module base of this context.
    /// </summary>
    IModuleBase? ModuleBase { get; set; }

    /// <summary>
    ///     Resets contextual execution data to initial values.
    /// </summary>
    ValueTask ResetAsync();
}
