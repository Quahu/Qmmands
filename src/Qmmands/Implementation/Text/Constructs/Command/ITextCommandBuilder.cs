using System;
using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a builder type for <see cref="ITextCommand"/>.
/// </summary>
public interface ITextCommandBuilder : ICommandBuilder
{
    /// <inheritdoc cref="ICommand.Module"/>
    new ITextModuleBuilder Module { get; }

    IModuleBuilder ICommandBuilder.Module => Module;

    /// <inheritdoc cref="ICommand.Parameters"/>
    new IList<ITextParameterBuilder> Parameters { get; }

    /// <summary>
    ///     Gets the aliases of this command.
    /// </summary>
    IList<string> Aliases { get; }

    /// <summary>
    ///     Gets or sets the overload priority of this command.
    /// </summary>
    /// <remarks>
    ///     Higher value means higher priority.
    ///     This is only used when determining the execution order of overloaded commands.
    /// </remarks>
    int OverloadPriority { get; set; }

    /// <summary>
    ///     Gets or sets whether this command ignores extra arguments.
    /// </summary>
    bool IgnoresExtraArguments { get; set; }

    /// <summary>
    ///     Gets or sets the argument parser type overriding the default of this command.
    /// </summary>
    Type? CustomArgumentParserType { get; set; }

    ITextCommand Build(ITextModule module);

    ICommand ICommandBuilder.Build(IModule module)
        => Build((ITextModule) module);
}
