using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal interface ITypeParser
    {
        Task<TypeParserResult<object>> ParseAsync(Parameter parameter, string value, ICommandContext context, IServiceProvider provider);
    }
}
