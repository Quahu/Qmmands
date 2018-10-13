using System;
using System.Threading.Tasks;

namespace Qmmands
{
    internal sealed class NullableTypeParser<T> : TypeParser<T> where T : struct
    {
        private readonly TypeParser<T> _typeParser;

        public NullableTypeParser(TypeParser<T> typeParser)
            => _typeParser = typeParser;

        public override Task<TypeParserResult<T>> ParseAsync(string input, ICommandContext context, IServiceProvider provider)
        {
            if (input.Equals("null", StringComparison.OrdinalIgnoreCase)
                || input.Equals("none", StringComparison.OrdinalIgnoreCase)
                || input.Equals("nothing", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new TypeParserResult<T>(true));

            return _typeParser.ParseAsync(input, context, provider);
        }
    }
}
