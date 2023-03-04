using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qommon;
using Qommon.Collections.ThreadSafe;

namespace Qmmands.Default;

/// <inheritdoc/>
public class DefaultTypeParserProvider : ITypeParserProvider
{
    /// <summary>
    ///     Gets the dictionary of <see cref="TryParseDelegate{T}"/>s keyed by the parsed types.
    /// </summary>
    public IThreadSafeDictionary<Type, Delegate> TryParseDelegates { get; }

    /// <summary>
    ///     Gets the type parsers keyed by the parsed types.
    /// </summary>
    /// <remarks>
    ///     If a parameter does not have a custom type parser set
    ///     the <b>first</b> parser from the list will be used.
    /// </remarks>
    public IThreadSafeDictionary<Type, IList<ITypeParser>> TypeParsers { get; }

    public DefaultTypeParserProvider(
        IOptions<DefaultTypeParserServiceConfiguration> options,
        ILogger<DefaultTypeParserProvider> logger)
    {
        var configuration = options.Value;
        TryParseDelegates = ThreadSafeDictionary.Monitor.Create<Type, Delegate>(32);
        TypeParsers = ThreadSafeDictionary.ConcurrentDictionary.Create<Type, IList<ITypeParser>>();

        static bool TryParseChar(ReadOnlySpan<char> value, out char result)
        {
            if (value.Length == 1)
            {
                result = value[0];
                return true;
            }

            result = default;
            return false;
        }

        this.AddDelegate<char>(TryParseChar);
        this.AddDelegate<bool>(bool.TryParse);
        this.AddDelegate<Guid>(Guid.TryParse);

        this.AddNumberDelegate<byte>(byte.TryParse);
        this.AddNumberDelegate<sbyte>(sbyte.TryParse);
        this.AddNumberDelegate<short>(short.TryParse);
        this.AddNumberDelegate<ushort>(ushort.TryParse);
        this.AddNumberDelegate<int>(int.TryParse);
        this.AddNumberDelegate<uint>(uint.TryParse);
        this.AddNumberDelegate<long>(long.TryParse);
        this.AddNumberDelegate<ulong>(ulong.TryParse);
        this.AddNumberDelegate<float>(float.TryParse);
        this.AddNumberDelegate<double>(double.TryParse);
        this.AddNumberDelegate<decimal>(decimal.TryParse);
        this.AddNumberDelegate<Half>(Half.TryParse);
        this.AddNumberDelegate<nint>(nint.TryParse);
        this.AddNumberDelegate<nuint>(nuint.TryParse);
    }

    /// <inheritdoc/>
    public ITypeParser? GetParser(IParameter parameter)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowCustomTypeParserNotFound(IParameter parameter)
        {
            Throw.InvalidOperationException($"No custom type parser found for parameter {parameter.Name} ({parameter.CustomTypeParserType}).");
        }

        var parsedType = parameter.GetTypeInformation().ActualType;

        // If the parser list exists for the parsed type, try to return a parser from it.
        if (TypeParsers.TryGetValue(parsedType, out var parsers))
        {
            var customParserType = parameter.CustomTypeParserType;
            if (customParserType != null)
            {
                lock (parsers)
                {
                    var count = parsers.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var parser = parsers[i];
                        if (parser.GetType() == customParserType)
                            return parser;
                    }
                }

                ThrowCustomTypeParserNotFound(parameter);
                return null;
            }

            lock (parsers)
            {
                if (parsers.Count != 0)
                {
                    return parsers[0];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ITypeParser CheckCustomParser(IParameter parameter, ITypeParser parser)
        {
            var customParserType = parameter.CustomTypeParserType;
            if (customParserType != null && customParserType != parser.GetType())
            {
                ThrowCustomTypeParserNotFound(parameter);
            }

            return parser;
        }

        // If it doesn't exist, but there's a try-parse delegate, add a new primitive type parser.
        if (TryParseDelegates.TryGetValue(parsedType, out var @delegate))
        {
            // Assumes that the user didn't add wrong delegate types.
            var parserType = @delegate.GetType().GetGenericTypeDefinition() == typeof(TryParseNumberDelegate<>)
                ? typeof(PrimitiveNumberTypeParser<>)
                : typeof(PrimitiveTypeParser<>);

            var parser = Activator.CreateInstance(parserType.MakeGenericType(parsedType), @delegate) as ITypeParser;
            Debug.Assert(parser != null);

            this.AddParser(parser);
            return CheckCustomParser(parameter, parser);
        }

        // If the type is an enum - add a new enum type parser.
        if (parsedType.IsEnum)
        {
            var parser = Activator.CreateInstance(typeof(EnumTypeParser<>).MakeGenericType(parsedType)) as ITypeParser;
            Debug.Assert(parser != null);

            this.AddParser(parser);
            return CheckCustomParser(parameter, parser);
        }

        return null;
    }
}
