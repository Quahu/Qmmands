using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands.Delegates;
using Qommon.Collections;
using Qommon.Events;

namespace Qmmands
{
    /// <summary>
    ///     Represents the extracted interface from <see cref="CommandService"/>.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        ///     Gets the <see cref="System.StringComparison"/> used for finding <see cref="Command"/>s and <see cref="Module"/>s,
        ///     used by the default <see langword="enum"/> parsers, and comparing <see cref="NullableNouns"/>.
        /// </summary>
        StringComparison StringComparison { get; }

        /// <summary>
        ///     Gets the default <see cref="RunMode"/> for commands and modules.
        /// </summary>
        RunMode DefaultRunMode { get; }

        /// <summary>
        ///     Gets whether commands ignore extra arguments by default or not.
        /// </summary>
        bool IgnoresExtraArguments { get; }

        /// <summary>
        ///     Gets the separator.
        /// </summary>
        string Separator { get; }

        /// <summary>
        ///     Gets the separator requirement.
        /// </summary>
        SeparatorRequirement SeparatorRequirement { get; }

        /// <summary>
        ///     Gets the default argument parser.
        /// </summary>
        IArgumentParser DefaultArgumentParser { get; }

        /// <summary>
        ///     Gets the generator <see langword="delegate"/> used for <see cref="Cooldown"/> bucket keys.
        /// </summary>
        CooldownBucketKeyGeneratorDelegate CooldownBucketKeyGenerator { get; }

        /// <summary>
        ///     Gets the quotation mark map used for non-remainder multi word arguments.
        /// </summary>
        IReadOnlyDictionary<char, char> QuotationMarkMap { get; }

        /// <summary>
        ///     Gets the collection of nouns used for nullable value type parsing.
        /// </summary>
        IReadOnlyList<string> NullableNouns { get; }

        /// <summary>
        ///     Gets the top-level modules.
        /// </summary>
        ReadOnlySet<Module> TopLevelModules { get; }

        /// <summary>
        ///     Fires after a <see cref="Command"/> was successfully executed.
        ///     You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        event AsynchronousEventHandler<CommandExecutedEventArgs> CommandExecuted;

        /// <summary>
        ///     Fires after a <see cref="Command"/> failed to execute.
        ///     You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        event AsynchronousEventHandler<CommandExecutionFailedEventArgs> CommandExecutionFailed;

        /// <summary>
        ///     Gets all of the added <see cref="Command"/>s.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Command"/>s.
        /// </returns>
        IReadOnlyList<Command> GetAllCommands();

        /// <summary>
        ///     Gets all of the added <see cref="Module"/>s.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Module"/>s.
        /// </returns>
        IReadOnlyList<Module> GetAllModules();

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     A list of <see cref="CommandMatch"/>es.
        /// </returns>
        IReadOnlyList<CommandMatch> FindCommands(string path);

        /// <summary>
        ///     Sets an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/> as the default parser.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        void SetDefaultArgumentParser<T>() where T : IArgumentParser;

        /// <summary>
        ///     Sets an <see cref="IArgumentParser"/> of the specified <see cref="Type"/> as the default parser.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        void SetDefaultArgumentParser(Type type);

        /// <summary>
        ///     Sets an <see cref="IArgumentParser"/> as the default parser.
        /// </summary>
        /// <param name="parser"> The <see cref="IArgumentParser"/> to set. </param>
        void SetDefaultArgumentParser(IArgumentParser parser);

        /// <summary>
        ///     Adds an <see cref="IArgumentParser"/>.
        /// </summary>
        /// <param name="parser"> The <see cref="IArgumentParser"/> to add. </param>
        void AddArgumentParser(IArgumentParser parser);

        /// <summary>
        ///     Removes an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        void RemoveArgumentParser<T>() where T : IArgumentParser;

        /// <summary>
        ///     Removes an <see cref="IArgumentParser"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        void RemoveArgumentParser(Type type);

        /// <summary>
        ///     Gets an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        /// <returns>
        ///     The <see cref="IArgumentParser"/> or <see langword="null"/>.
        /// </returns>
        IArgumentParser GetArgumentParser<T>() where T : IArgumentParser;

        /// <summary>
        ///     Gets an <see cref="IArgumentParser"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        /// <returns>
        ///     The <see cref="IArgumentParser"/> or <see langword="null"/>.
        /// </returns>
        IArgumentParser GetArgumentParser(Type type);

        /// <summary>
        ///     Adds a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The type to add the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to add for the <see cref="Type"/>. </param>
        /// <param name="replacePrimitive"> Whether to replace the primitive parser. </param>
        void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false);

        /// <summary>
        ///     Removes a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> to remove the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to remove for the <see cref="Type"/>. </param>
        void RemoveTypeParser<T>(TypeParser<T> parser);

        /// <summary>
        ///     Removes all added <see cref="TypeParser{T}"/>s.
        /// </summary>
        void RemoveAllTypeParsers();

        /// <summary>
        ///     Retrieves a <see cref="TypeParser{T}"/> from the added non-primitive parsers for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> the <see cref="TypeParser{T}"/> is for. </typeparam>
        /// <param name="replacingPrimitive"> Whether the <see cref="TypeParser{T}"/> replaces one of the primitive parsers or not. </param>
        /// <returns>
        ///     The <see cref="TypeParser{T}"/> or <see langword="null"/> if not found.
        /// </returns>
        TypeParser<T> GetTypeParser<T>(bool replacingPrimitive = false);

        /// <summary>
        ///     Retrieves a <see cref="TypeParser{T}"/> of the specified <typeparamref name="TParser"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> the <see cref="TypeParser{T}"/> is for. </typeparam>
        /// <typeparam name="TParser"> The <see cref="Type"/> of the <see cref="TypeParser{T}"/>. </typeparam>
        /// <returns>
        ///     The <see cref="TypeParser{T}"/> of the specified <typeparamref name="TParser"/> <see cref="Type"/> or <see langword="null"/> if not found.
        /// </returns>
        TParser GetSpecificTypeParser<T, TParser>() where TParser : TypeParser<T>;

        /// <summary>
        ///     Attempts to add all valid <see cref="Module"/>s and <see cref="Command"/>s found in the provided <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly"> The assembly to search. </param>
        /// <param name="predicate"> The optional <see cref="Predicate{T}"/> delegate that defines the conditions of the <see cref="Type"/>s to add as <see cref="Module"/>s. </param>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/>s before they are built. </param>
        /// <returns>
        ///     An <see cref="IReadOnlyList{Module}"/> of all found and added <see cref="Module"/>s.
        /// </returns>
        IReadOnlyList<Module> AddModules(Assembly assembly, Predicate<TypeInfo> predicate = null, Action<ModuleBuilder> action = null);

        /// <summary>
        ///     Attempts to instantiate, modify, and build a <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="action"> The action to perform on the builder. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule(Action<ModuleBuilder> action);

        /// <summary>
        ///     Adds the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to add. </param>
        void AddModule(Module module);

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The <see cref="Type"/> to add. </typeparam>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/> before it is built. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule<TModule>(Action<ModuleBuilder> action = null);

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> to add. </param>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/> before it is built. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule(Type type, Action<ModuleBuilder> action = null);

        /// <summary>
        ///     Removes all added <see cref="Module"/>s.
        /// </summary>
        void RemoveAllModules();

        /// <summary>
        ///     Removes the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to remove. </param>
        void RemoveModule(Module module);

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the input and executes the most suitable one.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        Task<IResult> ExecuteAsync(string input, CommandContext context);

        /// <summary>
        ///     Attempts to parse the arguments for the provided <see cref="Command"/> and execute it.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        Task<IResult> ExecuteAsync(Command command, string rawArguments, CommandContext context);
    }
}