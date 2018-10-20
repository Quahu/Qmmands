using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Qmmands
{
    /// <summary>
    ///     Provides some utility for handling commands.
    /// </summary>
    public static class CommandUtilities
    {
        /// <summary>
        ///     Represents a map of various quotation marks.
        /// </summary>
        public static IReadOnlyDictionary<char, char> QuoteMap { get; }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="char"/> prefix.
        ///     If it does, returns a trimmed <paramref name="output"/> <see cref="string"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="ignoreCase"> Whether to ignore casing or not. </param>
        /// <param name="output"> The trimmed output. Null if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasPrefix(string input, char prefix, bool ignoreCase, out string output)
        {
            if (input.Length == 0 || input[0] != (ignoreCase ? char.ToLowerInvariant(prefix) : prefix))
            {
                output = null;
                return false;
            }

            output = input.Substring(1).TrimStart();
            return true;
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="string"/> prefix.
        ///     If it does, returns a trimmed <paramref name="output"/> <see cref="string"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="stringComparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="output"> The trimmed output. Null if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasPrefix(string input, string prefix, StringComparison stringComparison, out string output)
        {
            if (!input.StartsWith(prefix, stringComparison))
            {
                output = null;
                return false;
            }

            output = input.Substring(prefix.Length).TrimStart();
            return true;
        }

        static CommandUtilities()
        {
            var quoteMapBuilder = ImmutableDictionary.CreateBuilder<char, char>();
            quoteMapBuilder['"'] = '"';
            quoteMapBuilder['«'] = '»';
            quoteMapBuilder['‘'] = '’';
            quoteMapBuilder['“'] = '”';
            quoteMapBuilder['„'] = '‟';
            quoteMapBuilder['‹'] = '›';
            quoteMapBuilder['‚'] = '‛';
            quoteMapBuilder['《'] = '》';
            quoteMapBuilder['〈'] = '〉';
            quoteMapBuilder['「'] = '」';
            quoteMapBuilder['『'] = '』';
            quoteMapBuilder['〝'] = '〞';
            quoteMapBuilder['﹁'] = '﹂';
            quoteMapBuilder['﹃'] = '﹄';
            quoteMapBuilder['＂'] = '＂';
            quoteMapBuilder['＇'] = '＇';
            quoteMapBuilder['｢'] = '｣';
            quoteMapBuilder['('] = ')';
            quoteMapBuilder['༺'] = '༻';
            quoteMapBuilder['༼'] = '༽';
            quoteMapBuilder['᚛'] = '᚜';
            quoteMapBuilder['⁅'] = '⁆';
            quoteMapBuilder['⌈'] = '⌉';
            quoteMapBuilder['⌊'] = '⌋';
            quoteMapBuilder['❨'] = '❩';
            quoteMapBuilder['❪'] = '❫';
            quoteMapBuilder['❬'] = '❭';
            quoteMapBuilder['❮'] = '❯';
            quoteMapBuilder['❰'] = '❱';
            quoteMapBuilder['❲'] = '❳';
            quoteMapBuilder['❴'] = '❵';
            quoteMapBuilder['⟅'] = '⟆';
            quoteMapBuilder['⟦'] = '⟧';
            quoteMapBuilder['⟨'] = '⟩';
            quoteMapBuilder['⟪'] = '⟫';
            quoteMapBuilder['⟬'] = '⟭';
            quoteMapBuilder['⟮'] = '⟯';
            quoteMapBuilder['⦃'] = '⦄';
            quoteMapBuilder['⦅'] = '⦆';
            quoteMapBuilder['⦇'] = '⦈';
            quoteMapBuilder['⦉'] = '⦊';
            quoteMapBuilder['⦋'] = '⦌';
            quoteMapBuilder['⦍'] = '⦎';
            quoteMapBuilder['⦏'] = '⦐';
            quoteMapBuilder['⦑'] = '⦒';
            quoteMapBuilder['⦓'] = '⦔';
            quoteMapBuilder['⦕'] = '⦖';
            quoteMapBuilder['⦗'] = '⦘';
            quoteMapBuilder['⧘'] = '⧙';
            quoteMapBuilder['⧚'] = '⧛';
            quoteMapBuilder['⧼'] = '⧽';
            quoteMapBuilder['⸂'] = '⸃';
            quoteMapBuilder['⸄'] = '⸅';
            quoteMapBuilder['⸉'] = '⸊';
            quoteMapBuilder['⸌'] = '⸍';
            quoteMapBuilder['⸜'] = '⸝';
            quoteMapBuilder['⸠'] = '⸡';
            quoteMapBuilder['⸢'] = '⸣';
            quoteMapBuilder['⸤'] = '⸥';
            quoteMapBuilder['⸦'] = '⸧';
            quoteMapBuilder['⸨'] = '⸩';
            quoteMapBuilder['【'] = '】';
            quoteMapBuilder['〔'] = '〕';
            quoteMapBuilder['〖'] = '〗';
            quoteMapBuilder['〘'] = '〙';
            quoteMapBuilder['〚'] = '〛';
            QuoteMap = quoteMapBuilder.ToImmutable();
        }
    }
}
