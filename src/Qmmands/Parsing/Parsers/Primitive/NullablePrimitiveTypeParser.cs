using System;

namespace Qmmands
{
    internal sealed class NullablePrimitiveTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly PrimitiveTypeParser<T> _primitiveTypeParser;

        public NullablePrimitiveTypeParser(PrimitiveTypeParser<T> primitiveTypeParser)
            => _primitiveTypeParser = primitiveTypeParser;

        public bool TryParse(string value, out T? result)
        {
            result = new T?();
            if (value.Equals("null", StringComparison.OrdinalIgnoreCase)
                || value.Equals("none", StringComparison.OrdinalIgnoreCase)
                || value.Equals("nothing", StringComparison.OrdinalIgnoreCase))
                return true;

            if (_primitiveTypeParser.TryParse(value, out var primitiveResult))
            {
                result = primitiveResult;
                return true;
            }

            return false;
        }

        bool IPrimitiveTypeParser.TryParse(string value, out object result)
        {
            result = new T?();
            if (TryParse(value, out var genericResult))
            {
                result = genericResult;
                return true;
            }

            return false;
        }
    }
}
