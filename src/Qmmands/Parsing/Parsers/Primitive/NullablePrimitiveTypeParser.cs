using System;
using System.Linq;

namespace Qmmands
{
    internal sealed class NullablePrimitiveTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly PrimitiveTypeParser<T> _primitiveTypeParser;

        public NullablePrimitiveTypeParser(PrimitiveTypeParser<T> primitiveTypeParser)
            => _primitiveTypeParser = primitiveTypeParser;

        public bool TryParse(CommandService service, string value, out T? result)
        {
            result = new T?();
            if (service.NullableNouns.Any(x => value.Equals(x, service.StringComparison)))
                return true;

            if (_primitiveTypeParser.TryParse(value, out var primitiveResult))
            {
                result = primitiveResult;
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
