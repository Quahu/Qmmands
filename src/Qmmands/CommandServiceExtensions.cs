using Microsoft.Extensions.DependencyInjection;

namespace Qmmands;

public static class CommandServiceExtensions
{
    public static ICommandMapProvider GetCommandMapProvider(this ICommandService commandService)
        => commandService.Services.GetRequiredService<ICommandMapProvider>();
}
