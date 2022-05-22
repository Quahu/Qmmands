using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Qmmands.Text;
using Qommon;

namespace Qmmands;

/// <summary>
///     Represents various Qmmands-related utilities.
/// </summary>
public static class CommandUtilities
{
    public static IDictionary<Type, string> FriendlyPrimitiveTypeNames { get; }

    static CommandUtilities()
    {
        FriendlyPrimitiveTypeNames = new Dictionary<Type, string>
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
            [typeof(decimal)] = "decimal",
            [typeof(nint)] = "nint",
            [typeof(nuint)] = "nuint"
        };
    }

    public static void ResetContext(ICommandContext context)
    {
        context.ExecutionStep = default;
        context.Command = default;
        context.RawArguments = default;
        context.Arguments = default;
        context.ModuleBase = default;
    }

    public static void ResetContext(ITextCommandContext context)
    {
        context.InputString = default;
        context.RawArgumentString = default;
        context.Path = default;
        context.IsOverloadDeterminant = default;

        ResetContext(context as ICommandContext);
    }

    public static bool HasPrefix(ReadOnlyMemory<char> input, ReadOnlySpan<char> prefix, StringComparison comparison, out ReadOnlyMemory<char> output)
    {
        var inputSpan = input.Span;
        if (!inputSpan.StartsWith(prefix, comparison))
        {
            output = default;
            return false;
        }

        output = input[prefix.Length..];
        return true;
    }

    // TODO: text utilities
    public static (bool HasRemainder, string Identifier) GetSignatureIdentifier(this ITextCommand command)
    {
        var hasRemainder = false;
        var stringBuilder = new StringBuilder();
        var parameters = command.Parameters;
        var parameterCount = parameters.Count;
        for (var i = 0; i < parameterCount; i++)
        {
            var parameter = parameters[i];
            hasRemainder = parameter is IPositionalParameter positionalParameter && positionalParameter.IsRemainder;
            stringBuilder.Append(parameter.ReflectedType).Append(';');
        }

        return (hasRemainder, stringBuilder.ToString());
    }

    private static ReadOnlySpan<char> Delimiters => new[] { ' ', '-', '_' };

    /// <summary>
    ///     Converts the input character span to <c>kebab-case</c> (the naming convention).
    /// </summary>
    /// <param name="text"> The text to convert. </param>
    /// <returns>
    ///     The converted text.
    /// </returns>
    public static string ToKebabCase(ReadOnlySpan<char> text)
    {
        return ToSeparatedCase(text, '-');
    }

    /// <summary>
    ///     Converts the input character span to <c>kebab-case</c> (the naming convention).
    /// </summary>
    /// <param name="text"> The text to convert. </param>
    /// <returns>
    ///     The converted text.
    /// </returns>
    public static string ToSnakeCase(ReadOnlySpan<char> text)
    {
        return ToSeparatedCase(text, '_');
    }

    public static string ToSeparatedCase(ReadOnlySpan<char> text, char separator)
    {
        if (text.IsEmpty)
            return string.Empty;

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];

            // Lowercases the first character.
            if (i == 0)
            {
                if (Delimiters.Contains(character))
                    continue;

                stringBuilder.Append(char.ToLowerInvariant(character));
                continue;
            }

            // Passes the separator through and replaces delimiters with the separator.
            if (character == separator || Delimiters.Contains(character))
            {
                if (stringBuilder.Length > 0 && stringBuilder[^1] != separator)
                    stringBuilder.Append(separator);

                continue;
            }

            // Skips lowercase characters.
            if (char.IsLower(character))
            {
                if (char.IsDigit(text[i - 1]))
                    stringBuilder.Append(separator);

                stringBuilder.Append(character);
                continue;
            }

            if (char.IsDigit(character))
            {
                // if current is a digit and previous wasn't a digit nor separator
                if (!char.IsDigit(text[i - 1]) && stringBuilder.Length > 0 && stringBuilder[^1] != separator)
                    stringBuilder.Append(separator);

                stringBuilder.Append(character);
                continue;
            }

            if (stringBuilder.Length > 0 && stringBuilder[^1] != separator // if previous wasn't separator
                && (char.IsLower(text[i - 1]) || char.IsDigit(text[i - 1]) // if previous was lowercase or digit
                    || i != text.Length - 1 && (char.IsLower(text[i + 1]) || char.IsDigit(text[i + 1])))) // if current is not last and next is lowercase or digit
            {
                stringBuilder.Append(separator);
            }

            stringBuilder.Append(char.ToLowerInvariant(character));
        }

        return stringBuilder.ToString();
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
    public static IEnumerable<ICheck> EnumerateAllChecks(IModule module)
    {
        Guard.IsNotNull(module);

        return GetAllChecksIterator(module);
    }

    private static IEnumerable<ICheck> GetAllChecksIterator(IModule module)
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
    public static IEnumerable<ICheck> EnumerateAllChecks(ICommand command)
    {
        Guard.IsNotNull(command);

        return GetAllChecksIterator(command);
    }

    private static IEnumerable<ICheck> GetAllChecksIterator(ICommand command)
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
    public static IEnumerable<ICommand> EnumerateAllCommands(IModule module)
    {
        Guard.IsNotNull(module);

        static IEnumerable<ICommand> GetCommands(IModule rModule)
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
    public static IEnumerable<ICommandBuilder> EnumerateAllCommands(IModuleBuilder moduleBuilder)
    {
        Guard.IsNotNull(moduleBuilder);

        static IEnumerable<ICommandBuilder> GetCommands(IModuleBuilder rModuleBuilder)
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
    public static IEnumerable<IModule> EnumerateAllSubmodules(IModule module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module), "The module must not be null.");

        static IEnumerable<IModule> GetModules(IModule rModule)
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
    public static IEnumerable<IModuleBuilder> EnumerateAllSubmodules(IModuleBuilder moduleBuilder)
    {
        Guard.IsNotNull(moduleBuilder);

        static IEnumerable<IModuleBuilder> GetSubmodules(IModuleBuilder rModuleBuilder)
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
}
