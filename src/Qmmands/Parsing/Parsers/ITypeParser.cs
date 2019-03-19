using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal interface ITypeParser
    {
#if NETCOREAPP
        ValueTask<TypeParserResult<object>>
#else
        Task<TypeParserResult<object>>
#endif
        ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider);
    }
}
