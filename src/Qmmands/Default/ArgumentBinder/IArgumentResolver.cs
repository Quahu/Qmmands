using System.Threading.Tasks;

namespace Qmmands.Default;

public interface IArgumentResolver
{
    ValueTask<IResult> ResolveAsync(ICommandContext context);
}