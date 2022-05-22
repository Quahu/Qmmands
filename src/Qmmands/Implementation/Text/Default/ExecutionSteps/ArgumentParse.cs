using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qommon;

namespace Qmmands.Text.Default;

public static partial class DefaultTextExecutionSteps
{
    public class ArgumentParse : CommandExecutionStep
    {
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.RawArguments != null;
        }

        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var textContext = Guard.IsAssignableToType<ITextCommandContext>(context);
            Guard.IsNotNull(textContext.InputString);

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

            var parameters = command.Parameters;
            var parameterCount = parameters.Count;
            if (parameterCount > 0)
            {
                var rawArguments = new Dictionary<IParameter, MultiString>(result.Arguments.Count);
                foreach (var (parameter, rawArgument) in result.Arguments)
                {
                    rawArguments[parameter] = rawArgument;
                }

                context.RawArguments = rawArguments;
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
