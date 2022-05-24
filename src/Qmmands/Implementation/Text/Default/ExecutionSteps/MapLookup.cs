using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;

namespace Qmmands.Text.Default;

public static partial class DefaultTextExecutionSteps
{
    /// <summary>
    ///     Finds <see cref="ICommandContext.Command"/> and extracts <see cref="ITextCommandContext.RawArgumentString"/> from <see cref="ITextCommandContext.InputString"/>.
    /// </summary>
    public class MapLookup : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.Command != null;
        }

        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var textContext = Guard.IsAssignableToType<ITextCommandContext>(context);
            Guard.IsNotNull(textContext.InputString);

            var mapProvider = context.Services.GetRequiredService<ICommandMapProvider>();
            var map = mapProvider.GetRequiredMap<ITextCommandMap>();
            var matches = map.FindMatches(textContext.InputString.Value);
            var matchCount = matches.Count;
            if (matchCount == 0)
                return CommandNotFoundResult.Instance;

            switch (matches)
            {
                case List<ITextCommandMatch> list:
                {
                    list.Sort(CommandOverloadComparer.Instance);
                    break;
                }
                case ITextCommandMatch[] array:
                {
                    Array.Sort(array, CommandOverloadComparer.Instance);
                    break;
                }
                default:
                {
                    var list = matches.ToList();
                    list.Sort(CommandOverloadComparer.Instance);
                    matches = list;
                    break;
                }
            }

            // TODO: command.IsDisabled
            Dictionary<ITextCommand, IResult>? failedOverloads = null;
            var pathLength = CommandOverloadComparer.GetPathLength(matches[0].Path);
            for (var i = 0; i < matchCount; i++)
            {
                var match = matches[i];
                if (i != 0 && CommandOverloadComparer.GetPathLength(match.Path) < pathLength)
                    continue;

                textContext.Command = match.Command;
                textContext.Path = match.Path;
                textContext.RawArgumentString = match.RawArgumentString;

                var result = await Next.ExecuteAsync(context).ConfigureAwait(false);
                if (result.IsSuccessful || matchCount == 1 || textContext.IsOverloadDeterminant || result is ExceptionResult)
                    return result;

                try
                {
                    await context.ResetAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return Results.Exception("resetting the context", ex);
                }

                (failedOverloads ??= new(4))[match.Command] = result;
            }

            if (failedOverloads!.Count == 1)
            {
                var enumerator = failedOverloads.GetEnumerator();
                enumerator.MoveNext();
                return enumerator.Current.Value;
            }

            return new OverloadsFailedResult(failedOverloads);
        }
    }
}
