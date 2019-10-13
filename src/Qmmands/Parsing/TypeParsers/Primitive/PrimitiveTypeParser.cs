using Qmmands.Delegates;

namespace Qmmands
{
    internal class PrimitiveTypeParser<T> : IPrimitiveTypeParser
        where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;

        public PrimitiveTypeParser()
        {
            _tryParse = (TryParseDelegate<T>) Utilities.TryParseDelegates[typeof(T)];
        }

        public bool TryParse(string value, out T result)
            => _tryParse(value, out result);

        bool IPrimitiveTypeParser.TryParse(Parameter parameter, string value, out object result)
        {
            if (!TryParse(value, out var genericResult))
            {
                result = null;
                return false;
            }

            result = genericResult;
            return true;
        }
    }
}
