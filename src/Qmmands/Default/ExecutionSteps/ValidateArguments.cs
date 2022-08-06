using System;
using System.Threading.Tasks;

namespace Qmmands.Default;

public static partial class DefaultExecutionSteps
{
    /// <summary>
    ///     Validates <see cref="ICommandContext.Arguments"/>.
    /// </summary>
    public class ValidateArguments : CommandExecutionStep
    {
        /// <inheritdoc />
        protected override bool CanBeSkipped(ICommandContext context)
        {
            return context.Arguments == null || context.Arguments.Count == 0;
        }

        /// <inheritdoc/>
        protected override async ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            foreach (var (parameter, argument) in context.Arguments!)
            {
                if (argument != null && !parameter.ReflectedType.IsInstanceOfType(argument))
                    throw new InvalidOperationException($"Value of type {argument.GetType()} is not assignable to the parameter {parameter.Name} ({parameter.ReflectedType}).");
            }

            return await Next.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
