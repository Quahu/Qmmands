using System.Collections.Generic;

namespace Qmmands.Text;

/// <summary>
///     Represents a text-based command module.
/// </summary>
public interface ITextModule : IModule
{
    /// <inheritdoc cref="IModule.Parent"/>
    new ITextModule? Parent { get; }

    IModule? IModule.Parent => Parent;

    /// <inheritdoc cref="IModule.Submodules"/>
    new IReadOnlyList<ITextModule> Submodules { get; }

    IReadOnlyList<IModule> IModule.Submodules => Submodules;

    /// <inheritdoc cref="IModule.Commands"/>
    new IReadOnlyList<ITextCommand> Commands { get; }

    IReadOnlyList<ICommand> IModule.Commands => Commands;

    /// <summary>
    ///     Gets the aliases of this module.
    /// </summary>
    IReadOnlyList<string> Aliases { get; }
}
