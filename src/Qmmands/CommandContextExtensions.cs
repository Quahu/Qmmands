using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Qmmands;

/// <summary>
///     Represents extension methods for <see cref="ICommandContext"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandContextExtensions
{
    public static ICommandService GetCommandService(this ICommandContext context)
    {
        return context.Services.GetRequiredService<ICommandService>();
    }
}