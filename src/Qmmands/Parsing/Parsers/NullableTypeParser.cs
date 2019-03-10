using System;
using System.Linq;
using System.Threading.Tasks;

namespace Qmmands
{
    internal sealed class NullableTypeParser<T> : TypeParser<T>
        where T : struct
    {
        private readonly TypeParser<T> _typeParser;

        public NullableTypeParser(TypeParser<T> typeParser)
        {
            _typeParser = typeParser;
        }

        public override Task<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, ICommandContext context, IServiceProvider provider)
            => parameter.Service.NullableNouns.Any(x => value.Equals(x, parameter.Service.StringComparison))
                ? Task.FromResult(new TypeParserResult<T>(false))
                : _typeParser.ParseAsync(parameter, value, context, provider);
    }
}
