using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal interface ITypeParser
    {
        Task<TypeParserResult<object>> ParseAsync(string value, ICommandContext context, IServiceProvider provider);
    }
}
