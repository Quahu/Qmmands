using System.Threading.Tasks;

namespace Qmmands
{
    internal interface IModuleBase
    {
        ValueTask BeforeExecutedAsync();

        ValueTask AfterExecutedAsync();

        void Prepare(CommandContext context);
    }
}