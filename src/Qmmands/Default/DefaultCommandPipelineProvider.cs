using Qommon.Collections.Proxied;

namespace Qmmands.Default;

/// <inheritdoc cref="ICommandPipelineProvider"/>
public class DefaultCommandPipelineProvider : ProxiedList<ICommandPipeline>, ICommandPipelineProvider
{
    public DefaultCommandPipelineProvider()
    { }

    public virtual ICommandPipeline? GetPipeline(ICommandContext context)
    {
        var pipelines = List;
        var count = pipelines.Count;
        for (var i = 0; i < count; i++)
        {
            var pipeline = pipelines[i];
            if (!pipeline.CanExecute(context))
                continue;

            return pipeline;
        }

        return null;
    }
}
