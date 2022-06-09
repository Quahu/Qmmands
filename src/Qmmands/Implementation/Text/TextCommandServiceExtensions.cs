using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Qmmands.Text;

/// <summary>
///     Represents text extensions for <see cref="ICommandService"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextCommandServiceExtensions
{
    /// <summary>
    ///     Enumerates top-level text modules added to this command service.
    /// </summary>
    /// <param name="commandService"> This command service. </param>
    /// <returns>
    ///     An enumerable of the modules.
    /// </returns>
    public static IEnumerable<ITextModule> EnumerateTextModules(this ICommandService commandService)
    {
        static IEnumerable<ITextModule> YieldModules(IEnumerable<IModule> modules)
        {
            foreach (var module in modules)
            {
                yield return (ITextModule) module;
            }
        }

        foreach (var kvp in commandService.EnumerateModules())
        {
            if (!typeof(ITextCommandMap).IsAssignableFrom(kvp.Key))
                continue;

            return YieldModules(kvp.Value);
        }

        return Array.Empty<ITextModule>();
    }
}
