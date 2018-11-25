namespace Qmmands
{
    internal delegate bool TryParseDelegate<T>(string value, out T result);

    internal class PrimitiveTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;

        public PrimitiveTypeParser()
            => _tryParse = (TryParseDelegate<T>) ReflectionUtilities.TryParseDelegates[typeof(T)];

        public bool TryParse(string values, out T result)
            => _tryParse(values, out result);

        bool IPrimitiveTypeParser.TryParse(CommandService service, string value, out object result)
        {
            result = default;
            if (!TryParse(value, out var genericResult))
                return false;

            result = genericResult;
            return true;
        }
    }
}
