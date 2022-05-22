using Microsoft.Extensions.DependencyInjection;
using Qmmands.Default;
using Qmmands.DependencyInjection.Extensions;

namespace Qmmands;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandService, DefaultCommandService>();

        services.AddCommandReflectorProvider();
        services.AddCommandMapProvider();
        services.AddTypeParserProvider();
        services.AddArgumentBinder();
        services.AddCommandPipelineProvider();
        services.AddCommandRateLimiter();

        return services;
    }

    public static IServiceCollection AddCommandReflectorProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandReflectorProvider, DefaultCommandReflectorProvider>();
        return services;
    }

    public static IServiceCollection AddArgumentBinder(this IServiceCollection services)
    {
        services.TryAddSingleton<IArgumentBinder, DefaultArgumentBinder>();
        return services;
    }

    public static IServiceCollection AddCommandMapProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandMapProvider, DefaultCommandMapProvider>();
        return services;
    }

    public static IServiceCollection AddTypeParserProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<ITypeParserProvider, DefaultTypeParserProvider>();
        return services;
    }

    public static IServiceCollection AddCommandPipelineProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandPipelineProvider, DefaultCommandPipelineProvider>();
        return services;
    }

    public static IServiceCollection AddCommandRateLimiter(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandRateLimiter, DefaultCommandRateLimiter>();
        return services;
    }
}
