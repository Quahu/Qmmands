using Microsoft.Extensions.DependencyInjection;
using Qmmands.DependencyInjection.Extensions;
using Qmmands.Text.Default;

namespace Qmmands.Text;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTextCommandService(this IServiceCollection services)
    {
        services.AddCommandService();

        services.AddArgumentParserProvider();

        return services;
    }

    public static IServiceCollection AddArgumentParserProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IArgumentParserProvider, DefaultArgumentParserProvider>();
        return services;
    }
}
