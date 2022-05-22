// using System;
// using System.Threading.Tasks;
// using Qommon;
//
// namespace Qmmands.Default
// {
//     /// <summary>
//     ///     Represents a type parser that handles nullable value types.
//     ///     If the value provided to parse is contained within <see cref="ICommandService.NullableNouns"/>, no value is returned.
//     ///     Otherwise, it is passed down to <see cref="UnderlyingTypeParser"/>.
//     /// </summary>
//     /// <typeparam name="T"> <inheritdoc/> </typeparam>
//     public class NullableTypeParser<T> : TypeParser<T>
//         where T : struct
//     {
//         /// <summary>
//         ///     Gets or sets the underlying parser.
//         /// </summary>
//         public ITypeParser<T> UnderlyingTypeParser { get; protected set; }
//
//         /// <summary>
//         ///     Instantiates a new <see cref="NullableTypeParser{T}"/>.
//         /// </summary>
//         /// <param name="underlyingTypeParser"> The underlying type parser. </param>
//         public NullableTypeParser(ITypeParser<T> underlyingTypeParser)
//         {
//             UnderlyingTypeParser = underlyingTypeParser;
//         }
//
//         /// <inheritdoc/>
//         public override ValueTask<ITypeParserResult<T>> ParseAsync(IParameter parameter, ReadOnlyMemory<char> value, ICommandContext context)
//         {
//             var nullableNouns = parameter.Service.NullableNouns;
//             var count = nullableNouns.Count;
//             for (var i = 0; i < count; i++)
//             {
//                 var nullableNoun = nullableNouns[i];
//                 if (!value.Span.Equals(nullableNoun, StringComparison.OrdinalIgnoreCase))
//                     continue;
//
//                 // We'll treat no value as a null value.
//                 return Success(Optional<T>.Empty);
//             }
//
//             return UnderlyingTypeParser.ParseAsync(parameter, value, context);
//         }
//     }
// }
