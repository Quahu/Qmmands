using System.Linq;

namespace Qmmands
{
    internal sealed class NullableEnumTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly EnumTypeParser<T> _enumTypeParser;

        public NullableEnumTypeParser(EnumTypeParser<T> enumTypeParser)
            => _enumTypeParser = enumTypeParser;

        public bool TryParse(CommandService service, string value, out object result)
        {
            result = null;
            if (service.NullableNouns.Any(x => value.Equals(x, service.StringComparison)))
                return true;

            if (_enumTypeParser.TryParse(service, value, out var enumResult))
            {
                result = enumResult;
                return true;
            }

            return false;
        }

        bool IPrimitiveTypeParser.TryParse(CommandService service, string value, out object result)
        {
            result = new T?();
            if (TryParse(service, value, out var genericResult))
            {
                result = genericResult;
                return true;
            }

            return false;
        }
    }
}
