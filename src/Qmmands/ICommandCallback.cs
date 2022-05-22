using System.Threading.Tasks;

namespace Qmmands;

/// <summary>
///     Represents the callback that will be executed
/// </summary>
public interface ICommandCallback
{
    ValueTask<IResult?> ExecuteAsync(ICommandContext context);
}
