using System;
using System.Collections.Generic;

namespace Qmmands
{
    // T is the underlying type of the enum, not typeof(enum)
    internal sealed class EnumTypeParser<T> : IPrimitiveTypeParser
        where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;
        private readonly Dictionary<string, object> _enumByNames;
        private readonly Dictionary<T, object> _enumByValues;

        public EnumTypeParser(Type enumType, bool ignoreCase)
        {
            _tryParse = (TryParseDelegate<T>) ReflectionUtilities.TryParseDelegates[typeof(T)];

            var names = Enum.GetNames(enumType);
            _enumByNames = new Dictionary<string, object>(names.Length, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            _enumByValues = new Dictionary<T, object>();
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = Enum.Parse(enumType, name);
                _enumByNames.Add(name, value);

                if (!_enumByValues.ContainsKey((T) value))
                    _enumByValues.Add((T) value, value);
            }
        }

        public bool TryParse(Parameter parameter, string value, out object result)
            => _tryParse(value, out var numericResult) ? _enumByValues.TryGetValue(numericResult, out result) : _enumByNames.TryGetValue(value, out result);
    }
}
