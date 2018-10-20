using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands
{
    internal sealed class EnumTypeParser<T> : IPrimitiveTypeParser where T : struct
    {
        private readonly TryParseDelegate<T> _tryParse;
        private readonly IReadOnlyDictionary<string, object> _enumByNames;
        private readonly IReadOnlyDictionary<T, object> _enumByValues;

        public EnumTypeParser(Type enumType, bool ignoreCase)
        {
            _tryParse = (TryParseDelegate<T>) TypeParserUtils.TryParseDelegates[typeof(T)];
            var enumValuesByNames = ImmutableDictionary.CreateBuilder<string, object>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            var enumValuesByValues = ImmutableDictionary.CreateBuilder<T, object>();

            var names = Enum.GetNames(enumType);
            for (var i = 0; i < names.Length; i++)
            {
                var name = ignoreCase ? names[i].ToLowerInvariant() : names[i];
                var value = Enum.Parse(enumType, name, ignoreCase);
                enumValuesByNames.Add(name, value);

                if (!enumValuesByValues.ContainsKey((T) value))
                    enumValuesByValues.Add((T) value, value);
            }

            _enumByNames = enumValuesByNames.ToImmutable();
            _enumByValues = enumValuesByValues.ToImmutable();
        }

        public bool TryParse(CommandService service, string value, out object result)
            => !_tryParse(value, out var numericResult) ? _enumByNames.TryGetValue(value, out result) : _enumByValues.TryGetValue(numericResult, out result);
    }
}
