using System.ComponentModel;

namespace Qmmands;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandPipelineExtensions
{
    public static ICommandPipeline Use(this ICommandPipeline pipeline, ICommandExecutionStep step)
    {
        pipeline.Add(step);
        return pipeline;
    }

    public static ICommandPipeline Use<TStep>(this ICommandPipeline pipeline)
        where TStep : ICommandExecutionStep, new()
    {
        pipeline.Add(new TStep());
        return pipeline;
    }
}
