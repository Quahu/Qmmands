using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Qommon.Collections;

namespace Qmmands;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CheckExtensions
{
    private static Task<KeyValuePair<ICheck, IResult>>[] ChecksToTasks(ICommandContext context, IReadOnlyList<ICheck> checks)
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

    public static async ValueTask<IResult> RunChecksAsync(this IModule module, ICommandContext context)
    {
        var parent = module.Parent;
        if (parent != null)
        {
            var result = await parent.RunChecksAsync(context).ConfigureAwait(false);
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

    public static async ValueTask<IResult> RunChecksAsync(this ICommand command, ICommandContext context)
    {
        var result = await command.Module.RunChecksAsync(context).ConfigureAwait(false);
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

        return Results.Success;
    }
}
