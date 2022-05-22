using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents a command management service.
/// </summary>
public interface ICommandService
{
    /// <summary>
    ///     Gets the services of this command service.
    /// </summary>
    IServiceProvider Services { get; }

    IEnumerable<KeyValuePair<Type, IEnumerable<IModule>>> EnumerateModules();

    IReadOnlyList<IModule> AddModules(Assembly assembly, Action<IModuleBuilder>? builderAction = null, Predicate<TypeInfo>? predicate = null);

    IModule AddModule(TypeInfo typeInfo, Action<IModuleBuilder>? builderAction = null);

    void AddModule(IModule module);

    bool RemoveModule(IModule module);

    void ClearModules(Type? mapType = null);

    /// <summary>
    ///     Executes the given command context.
    /// </summary>
    /// <param name="context"> The command context to execute. </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}"/> representing the parse operation with the result being an <see cref="IResult"/>.
    /// </returns>
    ValueTask<IResult> ExecuteAsync(ICommandContext context);
}
