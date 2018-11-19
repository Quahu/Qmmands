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
        public static IReadOnlyDictionary<char, char> DefaultQuoteMap { get; }

        /// <summary>
        ///     Represents a collection of nouns to use for nullable value type parsing.
        /// </summary>
        public static IReadOnlyList<string> DefaultNullableNouns { get; }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasPrefix(string input, char prefix, out string output)
            => HasPrefix(input, prefix, false, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="ignoreCase"> Whether to ignore casing or not. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
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
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix wasn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IReadOnlyList<char> prefixes, out char prefix, out string output)
            => HasAnyPrefix(input, prefixes, false, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="ignoreCase"> Whether to ignore casing or not. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix wasn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IReadOnlyList<char> prefixes, bool ignoreCase, out char prefix, out string output)
        {
            for (var i = 0; i < prefixes.Count; i++)
            {
                var currentPrefix = prefixes[i];
                if (!HasPrefix(input, currentPrefix, ignoreCase, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = default;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix wasn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IEnumerable<char> prefixes, out char prefix, out string output)
            => HasAnyPrefix(input, prefixes, false, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="ignoreCase"> Whether to ignore casing or not. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix wasn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IEnumerable<char> prefixes, bool ignoreCase, out char prefix, out string output)
        {
            foreach (var currentPrefix in prefixes)
            {
                if (!HasPrefix(input, currentPrefix, ignoreCase, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = default;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasPrefix(string input, string prefix, out string output)
            => HasPrefix(input, prefix, StringComparison.Ordinal, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="stringComparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
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

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix isn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IReadOnlyList<string> prefixes, out string prefix, out string output)
            => HasAnyPrefix(input, prefixes, StringComparison.Ordinal, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="stringComparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix isn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IReadOnlyList<string> prefixes, StringComparison stringComparison, out string prefix, out string output)
        {
            for (var i = 0; i < prefixes.Count; i++)
            {
                var currentPrefix = prefixes[i];
                if (!HasPrefix(input, currentPrefix, stringComparison, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = null;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix isn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IEnumerable<string> prefixes, out string prefix, out string output)
            => HasAnyPrefix(input, prefixes, StringComparison.Ordinal, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the specified <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="stringComparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix isn't found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns> A <see cref="bool"/> which determines whether the prefix was found or not. </returns>
        public static bool HasAnyPrefix(string input, IEnumerable<string> prefixes, StringComparison stringComparison, out string prefix, out string output)
        {
            foreach (var currentPrefix in prefixes)
            {
                if (!HasPrefix(input, currentPrefix, stringComparison, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = null;
            output = null;
            return false;
        }

        /// <summary>
        ///     Recursively gets all of the checks the specified <see cref="Module"/>
        ///     will require to pass before one of its <see cref="Command"/>s can be executed.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to get the checks for. </param>
        /// <returns> An enumerator of all checks. </returns>
        public static IEnumerable<CheckBaseAttribute> GetAllChecks(Module module)
        {
            if (module.Parent != null)
                foreach (var check in GetAllChecks(module.Parent))
                    yield return check;

            for (var i = 0; i < module.Checks.Count; i++)
                yield return module.Checks[i];
        }

        /// <summary>
        ///     Recursively gets all of the checks the specified <see cref="Command"/>
        ///     will require to pass before one of it can be executed.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to get the checks for. </param>
        /// <returns> An enumerator of all checks. </returns>
        public static IEnumerable<CheckBaseAttribute> GetAllChecks(Command command)
        {
            foreach (var check in GetAllChecks(command.Module))
                yield return check;

            for (var i = 0; i < command.Checks.Count; i++)
                yield return command.Checks[i];
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
            DefaultQuoteMap = quoteMapBuilder.ToImmutable();

            var nullableNounsBuilder = ImmutableArray.CreateBuilder<string>();
            nullableNounsBuilder.Add("null");
            nullableNounsBuilder.Add("none");
            nullableNounsBuilder.Add("nothing");
            DefaultNullableNouns = nullableNounsBuilder.ToImmutable();
        }
    }
}
