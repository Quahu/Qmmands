using System.Collections.Generic;
using System.Threading.Tasks;

namespace Qmmands;

public interface ICommandPipeline : IList<ICommandExecutionStep>
{
    bool CanExecute(ICommandContext context);

    ValueTask<IResult> ExecuteAsync(ICommandContext context);
}
