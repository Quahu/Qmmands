using System.Threading.Tasks;

namespace Qmmands
{
    internal interface IModuleBase
    {
        Task BeforeExecutedAsync();

        Task AfterExecutedAsync();

        void Prepare(CommandContext context);
    }
}