using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;

namespace Qmmands.Text.Default;

public static partial class DefaultTextExecutionSteps
{
    /// <summary>
    ///     Parses <see cref="ITextCommandContext.RawArgumentString"/> to <see cref="ICommandContext.RawArguments"/>.
    /// </summary>
    public class ArgumentParse : CommandExecutionStep
    {
        /// <inheritdoc/>
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.RawArguments != null;
        }

        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var textContext = Guard.IsAssignableToType<ITextCommandContext>(context);

            Guard.IsNotNull(textContext.Command);
            var command = textContext.Command;
            var argumentParserProvider = context.Services.GetRequiredService<IArgumentParserProvider>();
            var argumentParser = argumentParserProvider.GetParser(textContext.Command);

            Guard.IsNotNull(argumentParser);

            IArgumentParserResult result;
            try
            {
                result = await argumentParser.ParseAsync(textContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Results.Exception("argument parsing", ex);
            }

            if (!result.IsSuccessful)
                return result;

            context.Arguments = result.Arguments;
            context.RawArguments = result.RawArguments;

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
