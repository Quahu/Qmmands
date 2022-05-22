using System.Threading.Tasks;

namespace Qmmands;

// TODO: xmldocs
/// <summary>
///     Represents a type responsible for argument binding.
/// </summary>
public interface IArgumentBinder
{
    /// <summary>
    ///     Binds the arguments to parameter values.
    /// </summary>
    /// <param name="context">  </param>
    /// <returns></returns>
    ValueTask<IResult> BindAsync(ICommandContext context);
}
