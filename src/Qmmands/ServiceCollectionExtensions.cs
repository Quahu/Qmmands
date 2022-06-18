using Microsoft.Extensions.DependencyInjection;
using Qmmands.Default;
using Qmmands.DependencyInjection.Extensions;

namespace Qmmands;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandService, DefaultCommandService>();

        services.TryAddSingleton<ICommandReflectorProvider, DefaultCommandReflectorProvider>();
        services.TryAddSingleton<ICommandReflectorCallbackProvider, ReflectionCommandReflectorCallbackProvider>();
        services.TryAddSingleton<IArgumentBinder, DefaultArgumentBinder>();
        services.TryAddSingleton<ICommandMapProvider, DefaultCommandMapProvider>();
        services.TryAddSingleton<ITypeParserProvider, DefaultTypeParserProvider>();
        services.TryAddSingleton<ICommandPipelineProvider, DefaultCommandPipelineProvider>();
        services.TryAddSingleton<ICommandRateLimiter, DefaultCommandRateLimiter>();

        return services;
    }
}
