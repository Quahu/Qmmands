namespace Qmmands;

/// <summary>
///     Represents a type responsible for retrieving pipelines suitable for the execution
///     of specific contexts.
/// </summary>
public interface ICommandPipelineProvider
{
    /// <summary>
    ///     Gets a pipeline for the specified command context.
    /// </summary>
    /// <param name="context"> The context to get the pipeline for. </param>
    /// <returns>
    ///     A pipeline that will execute the command context.
    /// </returns>
    ICommandPipeline? GetPipeline(ICommandContext context);
}
