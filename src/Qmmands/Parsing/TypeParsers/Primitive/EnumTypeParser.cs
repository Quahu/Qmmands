using System;
using System.Collections.Generic;
using Qmmands.Delegates;

namespace Qmmands
{
    // T is the underlying type of the enum, not typeof(enum)
    internal sealed class EnumTypeParser<T> : IPrimitiveTypeParser
        where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;
        private readonly Dictionary<string, object> _enumByNames;
        private readonly Dictionary<T, object> _enumByValues;

        public EnumTypeParser(Type enumType, CommandService service)
        {
            _tryParse = (TryParseDelegate<T>) Utilities.TryParseDelegates[typeof(T)];

            var names = Enum.GetNames(enumType);
            _enumByNames = new Dictionary<string, object>(names.Length, service.StringComparer);
            _enumByValues = new Dictionary<T, object>();
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = Enum.Parse(enumType, name);
                _enumByNames[name] = value;
                _enumByValues[(T) value] = value;
            }
        }

        public bool TryParse(Parameter parameter, string value, out object result)
            => _tryParse(value, out var numericResult) ? _enumByValues.TryGetValue(numericResult, out result) : _enumByNames.TryGetValue(value, out result);
    }
}
