using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal interface ITypeParser
    {
        ValueTask<TypeParserResult<object>> ParseAsync(Parameter parameter, string value, CommandContext context);
    }
}
