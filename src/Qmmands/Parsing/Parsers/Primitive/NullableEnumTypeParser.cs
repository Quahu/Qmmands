using System;

namespace Qmmands
{
    internal sealed class NullableEnumTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly EnumTypeParser<T> _enumTypeParser;

        public NullableEnumTypeParser(EnumTypeParser<T> enumTypeParser)
            => _enumTypeParser = enumTypeParser;

        public bool TryParse(string value, out object result)
        {
            result = null;
            if (value.Equals("null", StringComparison.OrdinalIgnoreCase)
                || value.Equals("none", StringComparison.OrdinalIgnoreCase)
                || value.Equals("nothing", StringComparison.OrdinalIgnoreCase))
                return true;

            if (_enumTypeParser.TryParse(value, out var enumResult))
            {
                result = enumResult;
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
