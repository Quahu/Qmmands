using System;
using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a type that can be implemented by a <see langword="class"/> to make it and its members
///     get recognized as an <see cref="IModule"/> implementation.
/// </summary>
public interface IModuleBase
{
    /// <summary>
    ///     Gets or sets the command context of this module.
    /// </summary>
    /// <remarks>
    ///     Can return <see langword="null"/> if accessed inside the constructor.
    /// </remarks>
    ICommandContext Context { get; set; }

    /// <summary>
    ///     Gets the type of the command context expected by this module.
    /// </summary>
    Type ContextType { get; }

    /// <summary>
    ///     Called before a command in this module is executed.
    /// </summary>
    /// <remarks>
    ///     <see cref="Context"/> is available within this callback.
    /// </remarks>
    /// <returns>
    ///     A <see cref="ValueTask"/> representing the work.
    /// </returns>
    ValueTask OnBeforeExecuted();

    /// <summary>
    ///     Called after a command in this module is executed.
    /// </summary>
    /// <remarks>
    ///     <see cref="Context"/> is available within this callback.
    /// </remarks>
    /// <returns>
    ///     A <see cref="ValueTask"/> representing the work.
    /// </returns>
    ValueTask OnAfterExecuted();
}
