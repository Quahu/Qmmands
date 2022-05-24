using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Runs command checks.
    /// </summary>
    public class RunChecks : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            var command = context.Command;
            var result = await command.RunChecksAsync(context).ConfigureAwait(false);
            if (!result.IsSuccessful)
                return result;

            return await Next.ExecuteAsync(context);
        }
    }
}
