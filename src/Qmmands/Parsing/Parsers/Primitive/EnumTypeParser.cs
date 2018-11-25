using System;
using System.Collections.Generic;

namespace Qmmands
{
    internal sealed class EnumTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;
        private readonly Dictionary<string, object> _enumByNames;
        private readonly Dictionary<T, object> _enumByValues;

        public EnumTypeParser(Type enumType, bool ignoreCase)
        {
            _tryParse = (TryParseDelegate<T>) ReflectionUtilities.TryParseDelegates[typeof(T)];
            _enumByNames = new Dictionary<string, object>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            _enumByValues = new Dictionary<T, object>();

            var names = Enum.GetNames(enumType);
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = Enum.Parse(enumType, name);
                _enumByNames.Add(name, value);

                if (!_enumByValues.ContainsKey((T) value))
                    _enumByValues.Add((T) value, value);
            }
        }

        public bool TryParse(CommandService service, string value, out object result)
            => !_tryParse(value, out var numericResult) ? _enumByNames.TryGetValue(value, out result) : _enumByValues.TryGetValue(numericResult, out result);
    }
}
