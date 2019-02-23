using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Provides a framework interface for creating text based commands.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        ///     Fires when a command is successfully executed. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        event CommandExecutedDelegate CommandExecuted;

        /// <summary>
        ///     Fires when a command fails to execute. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        event CommandErroredDelegate CommandErrored;

        /// <summary>
        ///     Fires when a non-user instantiated <see cref="ModuleBuilder"/> is about to be built into a <see cref="Module"/>.
        /// </summary>
        event ModuleBuildingDelegate ModuleBuilding;

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
        ///     Attempts to find <see cref="Module"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     A list of <see cref="ModuleMatch"/>es.
        /// </returns>
        IReadOnlyList<ModuleMatch> FindModules(string path);

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
        /// <returns>
        ///     An <see cref="IReadOnlyList{Module}"/> of all found and added <see cref="Module"/>s.
        /// </returns>
        IReadOnlyList<Module> AddModules(Assembly assembly, Predicate<TypeInfo> predicate = null);

        /// <summary>
        ///     Attempts to build the specified <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builder"> The builder to build. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule(ModuleBuilder builder);

        /// <summary>
        ///     Attempts to instantiate, modify, and build a <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule(Action<ModuleBuilder> builderAction);

        /// <summary>
        ///     Adds the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to add. </param>
        void AddModule(Module module);

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The <see cref="Type"/> to add. </typeparam>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule<TModule>();

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        Module AddModule(Type type);

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
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        Task<IResult> ExecuteAsync(string input, ICommandContext context, IServiceProvider provider = null);

        /// <summary>
        ///     Attempts to parse the arguments for the provided <see cref="Command"/> and execute it.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        Task<IResult> ExecuteAsync(Command command, string rawArguments, ICommandContext context, IServiceProvider provider = null);
    }
}