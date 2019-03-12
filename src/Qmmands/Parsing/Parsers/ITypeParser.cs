using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal interface ITypeParser
    {
        Task<TypeParserResult<object>> ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider);
    }
}
