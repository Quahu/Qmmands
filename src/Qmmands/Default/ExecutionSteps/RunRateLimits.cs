using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Runs rate-limits.
    /// </summary>
    public class RunRateLimits : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var rateLimitService = context.Services.GetService<ICommandRateLimiter>();
            if (rateLimitService != null)
            {
                var result = await rateLimitService.RateLimitAsync(context).ConfigureAwait(false);
                if (!result.IsSuccessful)
                    return result;
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
