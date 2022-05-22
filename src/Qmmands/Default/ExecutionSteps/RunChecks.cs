using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qommon;
using Qommon.Collections;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    public class RunChecks : CommandExecutionStep
    {
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            Guard.IsNotNull(context.Command);

            static Task<KeyValuePair<ICheck, IResult>>[] ChecksToTasks(ICommandContext context, IReadOnlyList<ICheck> checks)
            {
                static async Task<KeyValuePair<ICheck, IResult>> RunCheckAsync(ICommandContext context, ICheck check)
                {
                    var checkResult = await check.CheckAsync(context).ConfigureAwait(false);
                    return KeyValuePair.Create(check, checkResult);
                }

                var checkCount = checks.Count;
                var tasks = new Task<KeyValuePair<ICheck, IResult>>[checkCount];
                for (var i = 0; i < checkCount; i++)
                {
                    tasks[i] = RunCheckAsync(context, checks[i]);
                }

                return tasks;
            }

            static async ValueTask<IResult> RunChecksAsync(ICommandContext context, IModule module)
            {
                if (module.Parent != null)
                {
                    var result = await RunChecksAsync(context, module.Parent).ConfigureAwait(false);
                    if (!result.IsSuccessful)
                        return result;
                }

                if (module.Checks.Count > 0)
                {
                    var checkResults = await Task.WhenAll(ChecksToTasks(context, module.Checks)).ConfigureAwait(false);
                    var failedGroups = checkResults.GroupBy(x => x.Key.Group)
                        .Where(x => x.Key == null ? x.Any(y => !y.Value.IsSuccessful) : x.All(y => !y.Value.IsSuccessful)).ToArray();

                    if (failedGroups.Length > 0)
                        return new ChecksFailedResult(module, failedGroups.SelectMany(x => x).Where(x => !x.Value.IsSuccessful).ToDictionary());
                }

                return Results.Success;
            }

            var command = context.Command;
            var result = await RunChecksAsync(context, command.Module).ConfigureAwait(false);
            if (!result.IsSuccessful)
                return result;

            if (command.Checks.Count > 0)
            {
                var checkResults = await Task.WhenAll(ChecksToTasks(context, command.Checks)).ConfigureAwait(false);
                var failedGroups = checkResults.GroupBy(x => x.Key.Group)
                    .Where(x => x.Key == null ? x.Any(y => !y.Value.IsSuccessful) : x.All(y => !y.Value.IsSuccessful)).ToArray();

                if (failedGroups.Length > 0)
                    return new ChecksFailedResult(command, failedGroups.SelectMany(x => x).Where(x => !x.Value.IsSuccessful).ToDictionary());
            }

            return await Next.ExecuteAsync(context);
        }
    }
}
