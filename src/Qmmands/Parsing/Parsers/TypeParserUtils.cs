using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands
{
    internal static class TypeParserUtils
    {
        public static IReadOnlyDictionary<Type, Delegate> TryParseDelegates { get; }

        public static ImmutableDictionary<Type, string> FriendlyTypeNames { get; }

        public static IPrimitiveTypeParser CreatePrimitiveTypeParser(Type type)
            => Activator.CreateInstance(typeof(PrimitiveTypeParser<>).MakeGenericType(type)) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateEnumTypeParser(Type type, Type enumType, bool ignoreCase)
            => typeof(EnumTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new object[] { enumType, ignoreCase }) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateNullableEnumTypeParser(Type type, IPrimitiveTypeParser enumTypeParser)
            => typeof(NullableEnumTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new[] { enumTypeParser }) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateNullablePrimitiveTypeParser(Type type, IPrimitiveTypeParser primitiveTypeParser)
            => typeof(NullablePrimitiveTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new[] { primitiveTypeParser }) as IPrimitiveTypeParser;

        public static ITypeParser CreateNullableTypeParser(Type nullableType, CommandService service, ITypeParser typeParser)
            => typeof(NullableTypeParser<>).MakeGenericType(nullableType).GetConstructors()[0].Invoke(new object[] { service, typeParser }) as ITypeParser;

        static TypeParserUtils()
        {
            var tryParseDelegatesBuilder = ImmutableDictionary.CreateBuilder<Type, Delegate>();
            tryParseDelegatesBuilder[typeof(char)] = (TryParseDelegate<char>) char.TryParse;
            tryParseDelegatesBuilder[typeof(bool)] = (TryParseDelegate<bool>) bool.TryParse;
            tryParseDelegatesBuilder[typeof(byte)] = (TryParseDelegate<byte>) byte.TryParse;
            tryParseDelegatesBuilder[typeof(sbyte)] = (TryParseDelegate<sbyte>) sbyte.TryParse;
            tryParseDelegatesBuilder[typeof(short)] = (TryParseDelegate<short>) short.TryParse;
            tryParseDelegatesBuilder[typeof(ushort)] = (TryParseDelegate<ushort>) ushort.TryParse;
            tryParseDelegatesBuilder[typeof(int)] = (TryParseDelegate<int>) int.TryParse;
            tryParseDelegatesBuilder[typeof(uint)] = (TryParseDelegate<uint>) uint.TryParse;
            tryParseDelegatesBuilder[typeof(long)] = (TryParseDelegate<long>) long.TryParse;
            tryParseDelegatesBuilder[typeof(ulong)] = (TryParseDelegate<ulong>) ulong.TryParse;
            tryParseDelegatesBuilder[typeof(float)] = (TryParseDelegate<float>) float.TryParse;
            tryParseDelegatesBuilder[typeof(double)] = (TryParseDelegate<double>) double.TryParse;
            tryParseDelegatesBuilder[typeof(decimal)] = (TryParseDelegate<decimal>) decimal.TryParse;
            TryParseDelegates = tryParseDelegatesBuilder.ToImmutable();

            var friendlyTypeNamesBuilder = ImmutableDictionary.CreateBuilder<Type, string>();
            friendlyTypeNamesBuilder[typeof(char)] = "char";
            friendlyTypeNamesBuilder[typeof(bool)] = "bool";
            friendlyTypeNamesBuilder[typeof(byte)] = "byte";
            friendlyTypeNamesBuilder[typeof(sbyte)] = "signed byte";
            friendlyTypeNamesBuilder[typeof(short)] = "short";
            friendlyTypeNamesBuilder[typeof(ushort)] = "unsigned short";
            friendlyTypeNamesBuilder[typeof(int)] = "int";
            friendlyTypeNamesBuilder[typeof(uint)] = "unsigned int";
            friendlyTypeNamesBuilder[typeof(long)] = "long";
            friendlyTypeNamesBuilder[typeof(ulong)] = "unsigned long";
            friendlyTypeNamesBuilder[typeof(float)] = "float";
            friendlyTypeNamesBuilder[typeof(double)] = "double";
            friendlyTypeNamesBuilder[typeof(decimal)] = "decimal";
            FriendlyTypeNames = friendlyTypeNamesBuilder.ToImmutable();
        }
    }
}