﻿using System;
using System.Collections.Generic;
using Qommon;
using Qommon.Collections.ReadOnly;

namespace Qmmands
{
    /// <summary>
    ///     Provides various utility for commands.
    /// </summary>
    public static class CommandUtilities
    {
        /// <summary>
        ///     Represents a map of most common quotation marks.
        /// </summary>
        public static readonly IReadOnlyDictionary<char, char> DefaultQuotationMarkMap;

        /// <summary>
        ///     Represents a collection of nouns to use for nullable value type parsing.
        /// </summary>
        public static readonly IReadOnlyList<string> DefaultNullableNouns;

        /// <summary>
        ///     Represents a map of friendly names used for primitive <see cref="Type"/>s by <see cref="ArgumentParseFailedResult.FailureReason"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<Type, string> FriendlyPrimitiveTypeNames;

        /// <summary>
        ///     Gets the amount of primitive type parsers.
        /// </summary>
        public static int PrimitiveTypeParserCount => Utilities.TryParseDelegates.Count;

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the provided <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        public static bool HasPrefix(string input, char prefix, out string output)
            => HasPrefix(input != null ? input.AsSpan() : throw new ArgumentNullException(nameof(input), "The input must not be null."), prefix, false, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the provided <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        public static bool HasPrefix(string input, char prefix, bool isCaseSensitive, out string output)
        {
            Guard.IsNotNull(input);

            return HasPrefix(input.AsSpan(), prefix, isCaseSensitive, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IReadOnlyList<char> prefixes, out char prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, false, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IReadOnlyList<char> prefixes, bool isCaseSensitive, out char prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, isCaseSensitive, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IEnumerable<char> prefixes, out char prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, false, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IEnumerable<char> prefixes, bool isCaseSensitive, out char prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, isCaseSensitive, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the provided <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefix must not be null.
        /// </exception>
        public static bool HasPrefix(string input, string prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasPrefix(input.AsSpan(), prefix, StringComparison.Ordinal, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the provided <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefix must not be null.
        /// </exception>
        public static bool HasPrefix(string input, string prefix, StringComparison comparison, out string output)
        {
            Guard.IsNotNull(input);

            return HasPrefix(input.AsSpan(), prefix, comparison, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IReadOnlyList<string> prefixes, out string prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, StringComparison.Ordinal, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IReadOnlyList<string> prefixes, StringComparison comparison, out string prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, comparison, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IEnumerable<string> prefixes, out string prefix, out string output)
            => HasAnyPrefix(input.AsSpan(), prefixes, StringComparison.Ordinal, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(string input, IEnumerable<string> prefixes, StringComparison comparison, out string prefix, out string output)
        {
            Guard.IsNotNull(input);

            return HasAnyPrefix(input.AsSpan(), prefixes, comparison, out prefix, out output);
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with the provided <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        public static bool HasPrefix(ReadOnlySpan<char> input, char prefix, out string output)
            => HasPrefix(input, prefix, false, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with the provided <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        public static bool HasPrefix(ReadOnlySpan<char> input, char prefix, bool isCaseSensitive, out string output)
        {
            if (input.Length == 0 || input[0] != (isCaseSensitive ? prefix : char.ToLowerInvariant(prefix)))
            {
                output = null;
                return false;
            }

            output = new string(input.Slice(1).TrimStart());
            return true;
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IReadOnlyList<char> prefixes, out char prefix, out string output)
            => HasAnyPrefix(input, prefixes, false, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IReadOnlyList<char> prefixes, bool isCaseSensitive, out char prefix, out string output)
        {
            Guard.IsNotNull(prefixes);

            for (var i = 0; i < prefixes.Count; i++)
            {
                var currentPrefix = prefixes[i];
                if (!HasPrefix(input, currentPrefix, isCaseSensitive, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = default;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IEnumerable<char> prefixes, out char prefix, out string output)
            => HasAnyPrefix(input, prefixes, false, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="char"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="char"/> prefixes to check for. </param>
        /// <param name="isCaseSensitive"> Whether the check is case-sensitive. </param>
        /// <param name="prefix"> The found prefix. Default <see cref="char"/> if the prefix was not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IEnumerable<char> prefixes, bool isCaseSensitive, out char prefix, out string output)
        {
            Guard.IsNotNull(prefixes);

            foreach (var currentPrefix in prefixes)
            {
                if (!HasPrefix(input, currentPrefix, isCaseSensitive, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = default;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with the provided <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefix must not be null.
        /// </exception>
        public static bool HasPrefix(ReadOnlySpan<char> input, string prefix, out string output)
            => HasPrefix(input, prefix, StringComparison.Ordinal, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with the provided <see cref="string"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefix"> The <see cref="string"/> prefix to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefix must not be null.
        /// </exception>
        public static bool HasPrefix(ReadOnlySpan<char> input, string prefix, StringComparison comparison, out string output)
        {
            Guard.IsNotNull(prefix);

            if (!input.StartsWith(prefix, comparison))
            {
                output = null;
                return false;
            }

            output = new string(input.Slice(prefix.Length).TrimStart());
            return true;
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IReadOnlyList<string> prefixes, out string prefix, out string output)
            => HasAnyPrefix(input, prefixes, StringComparison.Ordinal, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IReadOnlyList<string> prefixes, StringComparison comparison, out string prefix, out string output)
        {
            Guard.IsNotNull(prefixes);

            for (var i = 0; i < prefixes.Count; i++)
            {
                var currentPrefix = prefixes[i];
                if (!HasPrefix(input, currentPrefix, comparison, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = null;
            output = null;
            return false;
        }

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IEnumerable<string> prefixes, out string prefix, out string output)
            => HasAnyPrefix(input, prefixes, StringComparison.Ordinal, out prefix, out output);

        /// <summary>
        ///     Checks if the provided <see cref="ReadOnlySpan{T}"/> starts with any of the provided <see cref="string"/> prefixes.
        ///     If it does, returns <see langword="true"/>, the found <paramref name="prefix"/>, and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="ReadOnlySpan{T}"/> to check. </param>
        /// <param name="prefixes"> The <see cref="string"/> prefixes to check for. </param>
        /// <param name="comparison"> The <see cref="StringComparison"/> to use when checking for the prefix. </param>
        /// <param name="prefix"> The found prefix. <see langword="null"/> if the prefix is not found. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix is not found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The prefixes must not be null.
        /// </exception>
        public static bool HasAnyPrefix(ReadOnlySpan<char> input, IEnumerable<string> prefixes, StringComparison comparison, out string prefix, out string output)
        {
            Guard.IsNotNull(prefixes);

            foreach (var currentPrefix in prefixes)
            {
                if (!HasPrefix(input, currentPrefix, comparison, out output))
                    continue;

                prefix = currentPrefix;
                return true;
            }

            prefix = null;
            output = null;
            return false;
        }

        /// <summary>
        ///     Recursively enumerates all of the checks the provided <see cref="Module"/>
        ///     will require to pass before one of its <see cref="Command"/>s can be executed.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to get the checks for. </param>
        /// <returns>
        ///     An enumerator of all <see cref="CheckAttribute"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The module must not be null.
        /// </exception>
        public static IEnumerable<CheckAttribute> EnumerateAllChecks(Module module)
        {
            Guard.IsNotNull(module);

            return GetAllChecksIterator(module);
        }

        private static IEnumerable<CheckAttribute> GetAllChecksIterator(Module module)
        {
            if (module.Parent != null)
            {
                foreach (var check in EnumerateAllChecks(module.Parent))
                    yield return check;
            }

            for (var i = 0; i < module.Checks.Count; i++)
                yield return module.Checks[i];
        }

        /// <summary>
        ///     Recursively enumerates all of the checks the provided <see cref="Command"/>
        ///     will require to pass before it can be executed.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to get the checks for. </param>
        /// <returns>
        ///     An enumerator of all <see cref="CheckAttribute"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command must not be null.
        /// </exception>
        public static IEnumerable<CheckAttribute> EnumerateAllChecks(Command command)
        {
            Guard.IsNotNull(command);

            return GetAllChecksIterator(command);
        }

        private static IEnumerable<CheckAttribute> GetAllChecksIterator(Command command)
        {
            foreach (var check in EnumerateAllChecks(command.Module))
                yield return check;

            for (var i = 0; i < command.Checks.Count; i++)
                yield return command.Checks[i];
        }

        /// <summary>
        ///     Recursively enumerates all of the <see cref="Command"/>s in the provided <see cref="Module"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="Command"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The module must not be null.
        /// </exception>
        public static IEnumerable<Command> EnumerateAllCommands(Module module)
        {
            Guard.IsNotNull(module);

            static IEnumerable<Command> GetCommands(Module rModule)
            {
                for (var i = 0; i < rModule.Commands.Count; i++)
                    yield return rModule.Commands[i];

                for (var i = 0; i < rModule.Submodules.Count; i++)
                {
                    foreach (var command in GetCommands(rModule.Submodules[i]))
                        yield return command;
                }
            }

            return GetCommands(module);
        }

        /// <summary>
        ///     Recursively enumerates all of the <see cref="CommandBuilder"/>s in the provided <see cref="ModuleBuilder"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="CommandBuilder"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The builder must not be null.
        /// </exception>
        public static IEnumerable<CommandBuilder> EnumerateAllCommands(ModuleBuilder moduleBuilder)
        {
            Guard.IsNotNull(moduleBuilder);

            static IEnumerable<CommandBuilder> GetCommands(ModuleBuilder rModuleBuilder)
            {
                for (var i = 0; i < rModuleBuilder.Commands.Count; i++)
                    yield return rModuleBuilder.Commands[i];

                for (var i = 0; i < rModuleBuilder.Submodules.Count; i++)
                {
                    foreach (var command in GetCommands(rModuleBuilder.Submodules[i]))
                        yield return command;
                }
            }

            return GetCommands(moduleBuilder);
        }

        /// <summary>
        ///     Recursively enumerates all of the submodules in the provided <see cref="Module"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="Module"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The module must not be null.
        /// </exception>
        public static IEnumerable<Module> EnumerateAllSubmodules(Module module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module), "The module must not be null.");

            static IEnumerable<Module> GetModules(Module rModule)
            {
                for (var i = 0; i < rModule.Submodules.Count; i++)
                {
                    var submodule = rModule.Submodules[i];
                    yield return submodule;

                    foreach (var subsubmodule in GetModules(submodule))
                        yield return subsubmodule;
                }
            }

            return GetModules(module);
        }

        /// <summary>
        ///     Recursively enumerates all of the submodules in the provided <see cref="ModuleBuilder"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="ModuleBuilder"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The builder must not be null.
        /// </exception>
        public static IEnumerable<ModuleBuilder> EnumerateAllSubmodules(ModuleBuilder moduleBuilder)
        {
            Guard.IsNotNull(moduleBuilder);

            static IEnumerable<ModuleBuilder> GetSubmodules(ModuleBuilder rModuleBuilder)
            {
                for (var i = 0; i < rModuleBuilder.Submodules.Count; i++)
                {
                    var submodule = rModuleBuilder.Submodules[i];
                    yield return submodule;

                    foreach (var subsubmodule in GetSubmodules(submodule))
                        yield return subsubmodule;
                }
            }

            return GetSubmodules(moduleBuilder);
        }

        static CommandUtilities()
        {
            DefaultQuotationMarkMap = new Dictionary<char, char>(3)
            {
                ['"'] = '"',
                ['“'] = '”',
                ['„'] = '‟'
            }.ReadOnly();

            DefaultNullableNouns = new[] { "null" }.ReadOnly();
            FriendlyPrimitiveTypeNames = new Dictionary<Type, string>(PrimitiveTypeParserCount)
            {
                [typeof(char)] = "char",
                [typeof(bool)] = "bool",
                [typeof(byte)] = "byte",
                [typeof(sbyte)] = "signed byte",
                [typeof(short)] = "short",
                [typeof(ushort)] = "unsigned short",
                [typeof(int)] = "int",
                [typeof(uint)] = "unsigned int",
                [typeof(long)] = "long",
                [typeof(ulong)] = "unsigned long",
                [typeof(float)] = "float",
                [typeof(double)] = "double",
                [typeof(decimal)] = "decimal"
            }.ReadOnly();
        }
    }
}
