using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Binds <see cref="ICommandContext.Arguments"/> accordingly with <see cref="ICommand.Parameters"/>.
    /// </summary>
    public class BindArguments : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var binder = context.Services.GetRequiredService<IArgumentBinder>();
            var result = await binder.BindAsync(context).ConfigureAwait(false);
            if (!result.IsSuccessful)
                return result;

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
