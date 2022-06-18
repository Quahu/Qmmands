using System;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Creates the module base.
    /// </summary>
    public class CreateModuleBase : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            try
            {
                var moduleBase = await context.Command.Callback.CreateModuleBase(context).ConfigureAwait(false);
                context.ModuleBase = moduleBase;
            }
            catch (Exception ex)
            {
                return Results.Exception("creating the module base", ex);
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
