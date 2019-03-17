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

#if NETCOREAPP
        public override ValueTask<TypeParserResult<T>>
#else
        public override Task<TypeParserResult<T>>
#endif
        ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider)
            => parameter.Service.NullableNouns.Any(x => value.Equals(x, parameter.Service.StringComparison))
#if NETCOREAPP
                ? new ValueTask<TypeParserResult<T>>(new TypeParserResult<T>(false))
#else
                ? Task.FromResult(new TypeParserResult<T>(false))
#endif
                : _typeParser.ParseAsync(parameter, value, context, provider);
    }
}
