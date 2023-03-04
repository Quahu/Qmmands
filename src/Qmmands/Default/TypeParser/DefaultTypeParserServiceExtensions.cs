using System;
using System.Collections.Generic;
using System.ComponentModel;
using Qommon;

namespace Qmmands.Default;

/// <summary>
///     Represents extension methods for <see cref="DefaultTypeParserProvider"/>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultTypeParserServiceExtensions
{
    public static void AddParser(this DefaultTypeParserProvider provider, ITypeParser parser)
    {
        var parsers = provider.TypeParsers.GetOrAdd(parser.ParsedType, _ => new List<ITypeParser>());
        lock (parsers)
        {
            parsers.Add(parser);
        }
    }

    public static void AddParserAsDefault(this DefaultTypeParserProvider provider, ITypeParser parser)
    {
        var parsers = provider.TypeParsers.GetOrAdd(parser.ParsedType, _ => new List<ITypeParser>());
        lock (parsers)
        {
            parsers.Insert(0, parser);
        }
    }

    /// <summary>
    ///     Adds a <see cref="TryParseDelegate{T}"/> and a <see cref="PrimitiveTypeParser{T}"/> for it.
    /// </summary>
    /// <param name="provider"> The type parser service. </param>
    /// <param name="delegate"> The delegate. </param>
    /// <typeparam name="T"> The type to parse to. </typeparam>
    public static void AddDelegate<T>(this DefaultTypeParserProvider provider, TryParseDelegate<T> @delegate)
    {
        Guard.IsNotNull(@delegate);

        provider.TryParseDelegates.Add(typeof(T), @delegate);
    }

    /// <summary>
    ///     Adds a <see cref="TryParseDelegate{T}"/> and a <see cref="PrimitiveTypeParser{T}"/> for it.
    /// </summary>
    /// <param name="provider"> The type parser service. </param>
    /// <param name="delegate"> The delegate. </param>
    /// <typeparam name="T"> The type to parse to. </typeparam>
    public static void AddNumberDelegate<T>(this DefaultTypeParserProvider provider, TryParseNumberDelegate<T> @delegate)
    {
        Guard.IsNotNull(@delegate);

        provider.TryParseDelegates.Add(typeof(T), @delegate);
    }

    /// <summary>
    ///     Adds an <see cref="EnumTypeParser{TEnum}"/> for the specified enum type.
    /// </summary>
    /// <typeparam name="TEnum"> The enum type to add the parser for. </typeparam>
    public static void AddEnum<TEnum>(this DefaultTypeParserProvider provider)
        where TEnum : struct, Enum
    {
        provider.AddParser(new EnumTypeParser<TEnum>());
    }

#if FEATURE_GENERIC_MATH
        /// <summary>
        ///     Adds a <see cref="TryParseDelegate{T}"/> that uses <see cref="ISpanParseable{T}"/> to the collection.
        /// </summary>
        /// <typeparam name="TParseable"> The parseable type. </typeparam>
        public static void AddParseable<TParseable>(this DefaultTypeParserService typeParserService)
            where T : ISpanParseable<TParseable>
        {
            static bool TryParse(ReadOnlySpan<char> value, out TParseable result)
            {
                return TParseable.TryParse(value, out result);
            }

            typeParserService.TryParseDelegates.Add(typeof(TParseable), new TryParseDelegate<TParseable>(TryParse));
        }
#endif
}
