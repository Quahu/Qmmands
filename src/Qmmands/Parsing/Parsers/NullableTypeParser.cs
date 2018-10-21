using System;
using System.Linq;
using System.Threading.Tasks;

namespace Qmmands
{
    internal sealed class NullableTypeParser<T> : TypeParser<T> where T : struct
    {
        private readonly CommandService _service;
        private readonly TypeParser<T> _typeParser;

        public NullableTypeParser(CommandService service, TypeParser<T> typeParser)
        {
            _service = service;
            _typeParser = typeParser;
        }

        public override Task<TypeParserResult<T>> ParseAsync(string input, ICommandContext context, IServiceProvider provider)
            => _service.NullableNouns.Any(x => input.Equals(x, _service.StringComparison))
                ? Task.FromResult(new TypeParserResult<T>(false))
                : _typeParser.ParseAsync(input, context, provider);
    }
}
