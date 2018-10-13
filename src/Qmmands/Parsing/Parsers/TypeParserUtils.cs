using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands
{
    internal static class TypeParserUtils
    {
        public static readonly IReadOnlyDictionary<Type, Delegate> TryParseDelegates;

        public static IPrimitiveTypeParser CreatePrimitiveTypeParser(Type type)
            => Activator.CreateInstance(typeof(PrimitiveTypeParser<>).MakeGenericType(type)) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateNullablePrimitiveTypeParser(Type type, IPrimitiveTypeParser primitiveTypeParser)
            => typeof(NullablePrimitiveTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new[] { primitiveTypeParser }) as IPrimitiveTypeParser;

        public static ITypeParser CreateNullableTypeParser(Type nullableType, ITypeParser typeParser)
            => typeof(NullableTypeParser<>).MakeGenericType(nullableType).GetConstructors()[0].Invoke(new[] { typeParser }) as ITypeParser;

        static TypeParserUtils()
        {
            var builder = ImmutableDictionary.CreateBuilder<Type, Delegate>();
            builder[typeof(char)] = (TryParseDelegate<char>) char.TryParse;
            builder[typeof(bool)] = (TryParseDelegate<bool>) bool.TryParse;
            builder[typeof(byte)] = (TryParseDelegate<byte>) byte.TryParse;
            builder[typeof(sbyte)] = (TryParseDelegate<sbyte>) sbyte.TryParse;
            builder[typeof(short)] = (TryParseDelegate<short>) short.TryParse;
            builder[typeof(ushort)] = (TryParseDelegate<ushort>) ushort.TryParse;
            builder[typeof(int)] = (TryParseDelegate<int>) int.TryParse;
            builder[typeof(uint)] = (TryParseDelegate<uint>) uint.TryParse;
            builder[typeof(long)] = (TryParseDelegate<long>) long.TryParse;
            builder[typeof(ulong)] = (TryParseDelegate<ulong>) ulong.TryParse;
            builder[typeof(float)] = (TryParseDelegate<float>) float.TryParse;
            builder[typeof(double)] = (TryParseDelegate<double>) double.TryParse;
            builder[typeof(decimal)] = (TryParseDelegate<decimal>) decimal.TryParse;
            TryParseDelegates = builder.ToImmutable();
        }
    }
}
