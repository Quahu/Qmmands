using System.Threading.Tasks;
using Qommon;

namespace Qmmands.Text.Default;

public static partial class DefaultTextExecutionSteps
{
    public class SetOverloadDeterminant : CommandExecutionStep
    {
        protected override ValueTask<IResult> OnExecuted(ICommandContext context)
        {
            var textContext = Guard.IsAssignableToType<ITextCommandContext>(context);
            textContext.IsOverloadDeterminant = true;
            return Next.ExecuteAsync(context);
        }
    }
}
