using System.Threading.Tasks;

namespace Qmmands
{
    internal interface IModuleBase
    {
        Task BeforeExecutedAsync(Command command);

        Task AfterExecutedAsync(Command command);

        void Prepare(ICommandContext context);
    }
}