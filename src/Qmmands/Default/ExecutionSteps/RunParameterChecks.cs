using System.Collections.Generic;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Runs parameter checks.
    /// </summary>
    public class RunParameterChecks : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.Arguments == null || context.Arguments.Count == 0;
        }

        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            var arguments = context.Arguments!;
            var parameters = context.Command.Parameters;
            var parameterCount = parameters.Count;
            for (var i = 0; i < parameterCount; i++)
            {
                var parameter = parameters[i];
                if (!arguments.TryGetValue(parameter, out var argument))
                    continue;

                var defaultValue = parameter.DefaultValue;
                if (defaultValue.Equals(argument))
                    continue;

                var checks = parameter.Checks;
                var checkCount = checks.Count;
                Dictionary<IParameterCheck, IResult>? failedChecks = null;
                for (var j = 0; j < checkCount; j++)
                {
                    var check = checks[j];
                    if (!check.CanCheck(parameter, argument))
                        continue;

                    var result = await check.CheckAsync(context, parameter, argument).ConfigureAwait(false);
                    if (!result.IsSuccessful)
                        (failedChecks ??= new())[check] = result;
                }

                if (failedChecks != null)
                    return new ParameterChecksFailedResult(parameter, argument, failedChecks);
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
