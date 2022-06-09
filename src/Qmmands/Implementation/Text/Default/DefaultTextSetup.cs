using Microsoft.Extensions.DependencyInjection;
using Qmmands.Default;

namespace Qmmands.Text.Default;

public static class DefaultTextSetup
{
    public static void Initialize(ICommandService service)
    {
        var services = service.Services;
        var reflectorProvider = services.GetRequiredService<ICommandReflectorProvider>() as DefaultCommandReflectorProvider;
        reflectorProvider?.AddReflector(ActivatorUtilities.CreateInstance<DefaultTextCommandReflector>(services));

        var pipelineProvider = services.GetRequiredService<ICommandPipelineProvider>() as DefaultCommandPipelineProvider;
        pipelineProvider?.AddTextPipeline();

        var mapProvider = services.GetRequiredService<ICommandMapProvider>() as DefaultCommandMapProvider;
        mapProvider?.Add(ActivatorUtilities.CreateInstance<DefaultTextCommandMap>(services));

        var argumentParserProvider = services.GetRequiredService<IArgumentParserProvider>() as DefaultArgumentParserProvider;
        argumentParserProvider?.Add(ActivatorUtilities.CreateInstance<DefaultArgumentParser>(services));
        argumentParserProvider?.Add(ActivatorUtilities.CreateInstance<ClassicArgumentParser>(services));
    }
}
