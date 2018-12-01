using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Command"/>'s callback method.
    /// </summary>
    /// <param name="command"> The currently executed <see cref="Command"/>. </param>
    /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
    /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
    /// <param name="arguments"> The parsed arguments. </param>
    /// <returns>
    ///     An <see cref="IResult"/>.
    /// </returns>
    public delegate Task<IResult> CommandCallbackDelegate(Command command, object[] arguments, ICommandContext context, IServiceProvider provider);
}
