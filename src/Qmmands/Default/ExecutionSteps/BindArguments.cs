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
    public class BindArguments : CommandExecutionStep
    {
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
