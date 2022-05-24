using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    public class ExecuteCommand : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            try
            {
                var result = await context.Command.Callback.ExecuteAsync(context).ConfigureAwait(false);
                return result ?? Results.Success;
            }
            catch (Exception ex)
            {
                return Results.Exception("executing the command callback", ex);
            }
        }
    }
}
