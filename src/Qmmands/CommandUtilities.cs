using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
        public static readonly IReadOnlyDictionary<char, char> DefaultQuotationMarkMap;

        /// <summary>
        ///     Represents a collection of nouns to use for nullable value type parsing.
        /// </summary>
        public static readonly IReadOnlyList<string> DefaultNullableNouns;

        /// <summary>
        ///     The friendly names used for primitive <see cref="Type"/>s by <see cref="ArgumentParseFailedResult.Reason"/>. 
        /// </summary>
        public static readonly IReadOnlyDictionary<Type, string> FriendlyPrimitiveTypeNames;

        /// <summary>
        ///     Checks if the provided <see cref="string"/> starts with the specified <see cref="char"/> prefix.
        ///     If it does, returns <see langword="true"/> and the trimmed <paramref name="output"/>.
        /// </summary>
        /// <param name="input"> The input <see cref="string"/> to check. </param>
        /// <param name="prefix"> The <see cref="char"/> prefix to check for. </param>
        /// <param name="output"> The trimmed output. <see langword="null"/> if the prefix isn't found. </param>
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     A <see cref="bool"/> which determines whether the prefix was found or not.
        /// </returns>
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
        /// <returns>
        ///     An enumerator of all <see cref="CheckBaseAttribute"/>s.
        /// </returns>
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
        /// <returns>
        ///     An enumerator of all <see cref="CheckBaseAttribute"/>s.
        /// </returns>
        public static IEnumerable<CheckBaseAttribute> GetAllChecks(Command command)
        {
            foreach (var check in GetAllChecks(command.Module))
                yield return check;

            for (var i = 0; i < command.Checks.Count; i++)
                yield return command.Checks[i];
        }

        /// <summary>
        ///     Recursively gets all of the <see cref="Command"/>s in the specified <see cref="Module"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="Command"/>s.
        /// </returns>
        public static IEnumerable<Command> GetAllCommands(Module module)
        {
            IEnumerable<Command> GetCommands(Module rModule)
            {
                for (var i = 0; i < rModule.Commands.Count; i++)
                    yield return rModule.Commands[i];

                for (var i = 0; i < rModule.Submodules.Count; i++)
                    foreach (var command in GetCommands(rModule.Submodules[i]))
                        yield return command;
            }

            foreach (var command in GetCommands(module))
                yield return command;
        }

        /// <summary>
        ///     Recursively gets all of the <see cref="CommandBuilder"/>s in the specified <see cref="ModuleBuilder"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="CommandBuilder"/>s.
        /// </returns>
        public static IEnumerable<CommandBuilder> GetAllCommands(ModuleBuilder moduleBuilder)
        {
            IEnumerable<CommandBuilder> GetCommands(ModuleBuilder rModuleBuilder)
            {
                for (var i = 0; i < rModuleBuilder.Commands.Count; i++)
                    yield return rModuleBuilder.Commands[i];

                for (var i = 0; i < rModuleBuilder.Submodules.Count; i++)
                    foreach (var command in GetCommands(rModuleBuilder.Submodules[i]))
                        yield return command;
            }

            foreach (var command in GetCommands(moduleBuilder))
                yield return command;
        }

        /// <summary>
        ///     Recursively gets all of the submodules in the specified <see cref="Module"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="Module"/>s.
        /// </returns>
        public static IEnumerable<Module> GetAllSubmodules(Module module)
        {
            IEnumerable<Module> GetModules(Module rModule)
            {
                for (var i = 0; i < rModule.Submodules.Count; i++)
                    yield return rModule.Submodules[i];

                for (var i = 0; i < rModule.Submodules.Count; i++)
                    foreach (var command in GetModules(rModule.Submodules[i]))
                        yield return command;
            }

            foreach (var submodule in GetModules(module))
                yield return submodule;
        }

        /// <summary>
        ///     Recursively gets all of the submodules in the specified <see cref="ModuleBuilder"/> and its submodules.
        /// </summary>
        /// <returns>
        ///     An enumerator of all <see cref="ModuleBuilder"/>s.
        /// </returns>
        public static IEnumerable<ModuleBuilder> GetAllSubmodules(ModuleBuilder moduleBuilder)
        {
            IEnumerable<ModuleBuilder> GetModules(ModuleBuilder rModuleBuilder)
            {
                for (var i = 0; i < rModuleBuilder.Submodules.Count; i++)
                    yield return rModuleBuilder.Submodules[i];

                for (var i = 0; i < rModuleBuilder.Submodules.Count; i++)
                    foreach (var command in GetModules(rModuleBuilder.Submodules[i]))
                        yield return command;
            }

            foreach (var submodule in GetModules(moduleBuilder))
                yield return submodule;
        }

        static CommandUtilities()
        {
            DefaultQuotationMarkMap = new ReadOnlyDictionary<char, char>(new Dictionary<char, char>
            {
                ['"'] = '"',
                ['“'] = '”',
                ['„'] = '‟'
            });

            var nullableNounsBuilder = ImmutableArray.CreateBuilder<string>();
            nullableNounsBuilder.Add("nil");
            nullableNounsBuilder.Add("none");
            nullableNounsBuilder.Add("nothing");
            nullableNounsBuilder.Add("null");
            nullableNounsBuilder.Add("undefined");
            DefaultNullableNouns = nullableNounsBuilder.ToImmutable();

            FriendlyPrimitiveTypeNames = new ReadOnlyDictionary<Type, string>(new Dictionary<Type, string>
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
            });
        }
    }
}
