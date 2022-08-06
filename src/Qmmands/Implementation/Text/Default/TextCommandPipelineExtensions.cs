using System.ComponentModel;
using Qmmands.Default;
using static Qmmands.Text.Default.DefaultTextExecutionSteps;
using static Qmmands.Default.DefaultExecutionSteps;

namespace Qmmands.Text.Default;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextCommandPipelineExtensions
{
    public static void AddTextPipeline(this DefaultCommandPipelineProvider pipelines)
    {
        var pipeline = new DefaultCommandPipeline<ITextCommandContext>()
            .Use<MapLookup>()
            .Use<RunChecks>()
            .Use<ArgumentParse>()
            .Use<TypeParse>()
            .Use<BindArguments>()
            .Use<RunParameterChecks>()
            .Use<ValidateArguments>()
            .Use<SetOverloadDeterminant>()
            .Use<RunRateLimits>()
            .Use<CreateModuleBase>()
            .Use<ExecuteCommand>();

        pipelines.Add(pipeline);
    }
}
