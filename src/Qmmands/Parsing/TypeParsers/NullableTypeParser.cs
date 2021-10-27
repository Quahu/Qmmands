using System.Collections.Immutable;
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

        public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            return parameter.Service.NullableNouns.Any(noun => value.Equals(noun, parameter.Service.StringComparison))
                ? new(new TypeParserResult<T>(false))
                : _typeParser.ParseAsync(parameter, value, context);
        }
    }
}
