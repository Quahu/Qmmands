using System.Linq;

namespace Qmmands
{
    internal sealed class NullablePrimitiveTypeParser<T> : IPrimitiveTypeParser
        where T : struct
    {
        private readonly PrimitiveTypeParser<T> _primitiveTypeParser;

        public NullablePrimitiveTypeParser(PrimitiveTypeParser<T> primitiveTypeParser)
        {
            _primitiveTypeParser = primitiveTypeParser;
        }

        public bool TryParse(Parameter parameter, string value, out object result)
        {
            if (parameter.Service.NullableNouns.Any(x => value.Equals(x, parameter.Service.StringComparison)))
            {
                result = new T?();
                return true;
            }

            if (!_primitiveTypeParser.TryParse(value, out var primitiveResult))
            {
                result = new T?();
                return false;
            }

            result = primitiveResult;
            return true;
        }
    }
}
