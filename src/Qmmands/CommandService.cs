using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Provides a framework for creating text based commands.
    /// </summary>
    public class CommandService : ICommandService
    {
        /// <summary>
        ///     Gets whether <see cref="FindModules"/>, <see cref="FindCommands"/> and primitive <see langword="enum"/> type parsers are case sensitive or not.
        /// </summary>
        public bool IsCaseSensitive { get; }

        /// <summary>
        ///     Gets the default <see cref="RunMode"/> for commands and modules.
        /// </summary>
        public RunMode DefaultRunMode { get; }

        /// <summary>
        ///     Gets whether commands should ignore extra arguments by default or not.
        /// </summary>
        public bool IgnoresExtraArguments { get; }

        /// <summary>
        ///     Gets the separator.
        /// </summary>
        public string Separator { get; }

        /// <summary>
        ///     Gets the separator requirement.
        /// </summary>
        public SeparatorRequirement SeparatorRequirement { get; }

        /// <summary>
        ///     Gets the argument parser.
        /// </summary>
        public IArgumentParser ArgumentParser { get; }

        /// <summary>
        ///     Gets the generator <see langword="delegate"/> to use for <see cref="Cooldown"/> bucket keys.
        /// </summary>
        public CooldownBucketKeyGeneratorDelegate CooldownBucketKeyGenerator { get; }

        /// <summary>
        ///     Gets the quotation mark map used for non-remainder multi word arguments.
        /// </summary>
        public IReadOnlyDictionary<char, char> QuotationMarkMap { get; }

        /// <summary>
        ///     Gets the collection of nouns used for nullable value type parsing.
        /// </summary>
        public IReadOnlyList<string> NullableNouns { get; }

        /// <summary>
        ///     Fires when a command is successfully executed. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        public event CommandExecutedDelegate CommandExecuted
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                lock (_handlerLock)
                    CommandExecutedHandlers = CommandExecutedHandlers.Add(value);
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                lock (_handlerLock)
                    CommandExecutedHandlers = CommandExecutedHandlers.Remove(value);
            }
        }

        /// <summary>
        ///     Gets the <see cref="CommandExecuted"/> event handlers.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImmutableArray<CommandExecutedDelegate> CommandExecutedHandlers { get; private set; } = ImmutableArray<CommandExecutedDelegate>.Empty;

        /// <summary>
        ///     Fires when a command fails to execute. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        public event CommandErroredDelegate CommandErrored
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                lock (_handlerLock)
                    CommandErroredHandlers = CommandErroredHandlers.Add(value);
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                lock (_handlerLock)
                    CommandErroredHandlers = CommandErroredHandlers.Remove(value);
            }
        }

        /// <summary>
        ///     Gets the <see cref="CommandErroredHandlers"/> event handlers.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImmutableArray<CommandErroredDelegate> CommandErroredHandlers { get; private set; } = ImmutableArray<CommandErroredDelegate>.Empty;

        internal StringComparison StringComparison { get; }

        private readonly ConcurrentDictionary<Type, Dictionary<Type, (bool ReplacingPrimitive, ITypeParser Instance)>> _typeParsers;
        private readonly ConcurrentDictionary<Type, IPrimitiveTypeParser> _primitiveTypeParsers;
        private readonly Dictionary<Type, Module> _typeModules;
        private readonly HashSet<Module> _modules;
        private readonly CommandMap _map;
        private static readonly Type _stringType = typeof(string);
        private readonly object _moduleLock = new object();
        private readonly object _handlerLock = new object();

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the specified <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        /// <param name="configuration"> The <see cref="CommandServiceConfiguration"/> to use. </param>
        /// <exception cref="ArgumentNullException">
        ///     The configuration must not be null.
        /// </exception>
        public CommandService(CommandServiceConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "The configuration must not be null.");

            IsCaseSensitive = configuration.CaseSensitive;
            DefaultRunMode = configuration.DefaultRunMode;
            IgnoresExtraArguments = configuration.IgnoreExtraArguments;
            Separator = configuration.Separator;
            SeparatorRequirement = configuration.SeparatorRequirement;
            ArgumentParser = configuration.ArgumentParser ?? DefaultArgumentParser.Instance;
            CooldownBucketKeyGenerator = configuration.CooldownBucketKeyGenerator;
            QuotationMarkMap = configuration.QuoteMap != null
                ? new ReadOnlyDictionary<char, char>(configuration.QuoteMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                : CommandUtilities.DefaultQuotationMarkMap;
            NullableNouns = configuration.NullableNouns != null
                ? configuration.NullableNouns.ToImmutableArray()
                : CommandUtilities.DefaultNullableNouns;

            StringComparison = IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            _typeModules = new Dictionary<Type, Module>();
            _modules = new HashSet<Module>();
            _map = new CommandMap(this);
            _typeParsers = new ConcurrentDictionary<Type, Dictionary<Type, (bool, ITypeParser)>>();
            _primitiveTypeParsers = new ConcurrentDictionary<Type, IPrimitiveTypeParser>(Environment.ProcessorCount, ReflectionUtilities.TryParseDelegates.Count * 2);
            foreach (var type in ReflectionUtilities.TryParseDelegates.Keys)
            {
                var primitiveTypeParser = ReflectionUtilities.CreatePrimitiveTypeParser(type);
                _primitiveTypeParsers.TryAdd(type, primitiveTypeParser);
                _primitiveTypeParsers.TryAdd(ReflectionUtilities.MakeNullable(type), ReflectionUtilities.CreateNullablePrimitiveTypeParser(type, primitiveTypeParser));
            }
        }

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the default <see cref="CommandServiceConfiguration"/>
        /// </summary>
        public CommandService() : this(CommandServiceConfiguration.Default)
        { }

        /// <summary>
        ///     Gets all of the added <see cref="Command"/>s.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Command"/>s.
        /// </returns>
        public IReadOnlyList<Command> GetAllCommands()
        {
            IEnumerable<Command> GetCommands(Module module)
            {
                for (var i = 0; i < module.Commands.Count; i++)
                    yield return module.Commands[i];

                for (var i = 0; i < module.Submodules.Count; i++)
                    foreach (var command in GetCommands(module.Submodules[i]))
                        yield return command;
            }

            lock (_moduleLock)
                return _modules.SelectMany(GetCommands).ToImmutableArray();
        }

        /// <summary>
        ///     Gets all of the added <see cref="Module"/>s.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Module"/>s.
        /// </returns>
        public IReadOnlyList<Module> GetAllModules()
        {
            IEnumerable<Module> GetSubmodules(Module module)
            {
                for (var i = 0; i < module.Submodules.Count; i++)
                {
                    var submodule = module.Submodules[i];
                    yield return submodule;

                    foreach (var subsubmodule in GetSubmodules(submodule))
                        yield return subsubmodule;
                }
            }

            lock (_moduleLock)
            {
                var builder = ImmutableArray.CreateBuilder<Module>();
                foreach (var module in _modules)
                {
                    builder.Add(module);
                    builder.AddRange(GetSubmodules(module));
                }

                return builder.TryMoveToImmutable();
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     An ordered list of <see cref="CommandMatch"/>es.
        /// </returns>
        public IReadOnlyList<CommandMatch> FindCommands(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path to find commands for must not be null.");

            lock (_moduleLock)
            {
                return _map.FindCommands(path).OrderByDescending(x => x.Path.Count)
                    .ThenByDescending(x => x.Command.Priority)
                    .ThenByDescending(x => x.Command.Parameters.Count)
                    .ToImmutableArray();
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Module"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     An ordered list of <see cref="ModuleMatch"/>es.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The path to find modules for must not be null.
        /// </exception>
        public IReadOnlyList<ModuleMatch> FindModules(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path to find modules for must not be null.");

            lock (_moduleLock)
            {
                return _map.FindModules(path)
                    .OrderByDescending(x => x.Path.Count)
                    .ToImmutableArray();
            }
        }

        /// <summary>
        ///     Adds a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The type to add the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to add for the <see cref="Type"/>. </param>
        /// <param name="replacePrimitive"> Whether to replace the primitive parser. </param>
        /// <exception cref="ArgumentNullException">
        ///     The type parser to add must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Custom enum type parsers are not supported.
        /// </exception>
        public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to add must not be null.");

            var type = typeof(T);
            if (ReflectionUtilities.IsNullable(type))
                throw new ArgumentException("Cannot add custom type parsers for nullable types.", nameof(T));

            if (replacePrimitive)
            {
                if (GetPrimitiveTypeParser(type) == null)
                    throw new ArgumentException($"No primitive type parser found to replace for type {type}.", nameof(T));

                var existingParser = GetAnyTypeParser(type, true);
                if (existingParser != null)
                    throw new ArgumentException($"There is already a custom type parser replacing the primitive parser for type {type} - {existingParser.GetType()}.");
            }

            _typeParsers.AddOrUpdate(type,
            new Dictionary<Type, (bool, ITypeParser)> { [parser.GetType()] = (replacePrimitive, parser) },
            (_, v) =>
            {
                v.Add(parser.GetType(), (replacePrimitive, parser));
                return v;
            });

            if (type.IsValueType)
            {
                var nullableParser = ReflectionUtilities.CreateNullableTypeParser(type, parser);
                _typeParsers.AddOrUpdate(ReflectionUtilities.MakeNullable(type),
                    new Dictionary<Type, (bool, ITypeParser)> { [nullableParser.GetType()] = (replacePrimitive, nullableParser) },
                    (_, v) =>
                    {
                        v.Add(nullableParser.GetType(), (replacePrimitive, nullableParser));
                        return v;
                    });
            }
        }

        /// <summary>
        ///     Removes a <see cref="TypeParser{T}"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> to remove the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to remove for the <see cref="Type"/>. </param>
        /// <exception cref="ArgumentNullException">
        ///     The type parser to remove must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The type parser to remove has not been added.
        /// </exception>
        public void RemoveTypeParser<T>(TypeParser<T> parser)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to remove must not be null.");

            var type = typeof(T);
            if (!_typeParsers.TryGetValue(type, out var typeParsers) || !typeParsers.Remove(parser.GetType()))
                throw new ArgumentException("The type parser to remove has not been added.");

            if (type.IsValueType)
                typeParsers.Remove(ReflectionUtilities.MakeNullable(type));
        }

        /// <summary>
        ///     Removes all added <see cref="TypeParser{T}"/>s.
        /// </summary>
        public void RemoveAllTypeParsers()
        {
            foreach (var typeParsers in _typeParsers.ToArray())
                _typeParsers.TryRemove(typeParsers.Key, out _);
        }

        /// <summary>
        ///     Retrieves a <see cref="TypeParser{T}"/> from the added non-primitive parsers for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> the <see cref="TypeParser{T}"/> is for. </typeparam>
        /// <param name="replacingPrimitive"> Whether the <see cref="TypeParser{T}"/> replaces one of the primitive parsers or not. </param>
        /// <returns>
        ///     The <see cref="TypeParser{T}"/> or <see langword="null"/> if not found.
        /// </returns>
        public TypeParser<T> GetTypeParser<T>(bool replacingPrimitive = false)
            => GetAnyTypeParser(typeof(T), replacingPrimitive) as TypeParser<T>;

        /// <summary>
        ///     Retrieves a <see cref="TypeParser{T}"/> of the specified <typeparamref name="TParser"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> the <see cref="TypeParser{T}"/> is for. </typeparam>
        /// <typeparam name="TParser"> The <see cref="Type"/> of the <see cref="TypeParser{T}"/>. </typeparam>
        /// <returns>
        ///     The <see cref="TypeParser{T}"/> of the specified <typeparamref name="TParser"/> <see cref="Type"/> or <see langword="null"/> if not found.
        /// </returns>
        public TParser GetSpecificTypeParser<T, TParser>() where TParser : TypeParser<T>
            => GetSpecificTypeParser(typeof(T), typeof(TParser)) as TParser;

        internal ITypeParser GetSpecificTypeParser(Type type, Type parserType)
            => _typeParsers.TryGetValue(type, out var typeParsers) && typeParsers.TryGetValue(parserType, out var typeParser) ? typeParser.Instance : null;

        internal ITypeParser GetAnyTypeParser(Type type, bool replacingPrimitive)
        {
            if (_typeParsers.TryGetValue(type, out var typeParsers))
            {
                if (replacingPrimitive)
                {
                    foreach (var parser in typeParsers.Values)
                    {
                        if (parser.ReplacingPrimitive)
                            return parser.Instance;
                    }
                }

                else
                    return typeParsers.First().Value.Instance;
            }

            return null;
        }

        internal IPrimitiveTypeParser GetPrimitiveTypeParser(Type type)
        {
            if (_primitiveTypeParsers.TryGetValue(type, out var typeParser))
                return typeParser;

            if (type.IsEnum)
            {
                var enumParser = ReflectionUtilities.CreateEnumTypeParser(type.GetEnumUnderlyingType(), type, !IsCaseSensitive);
                _primitiveTypeParsers.TryAdd(type, enumParser);
                _primitiveTypeParsers.TryAdd(ReflectionUtilities.MakeNullable(type), ReflectionUtilities.CreateNullableEnumTypeParser(type.GetEnumUnderlyingType(), enumParser));
                return enumParser;
            }

            return ReflectionUtilities.IsNullable(type) && (type = Nullable.GetUnderlyingType(type)).IsEnum
                ? GetPrimitiveTypeParser(type)
                : null;
        }

        /// <summary>
        ///     Attempts to add all valid <see cref="Module"/>s and <see cref="Command"/>s found in the provided <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly"> The assembly to search. </param>
        /// <param name="predicate"> The optional <see cref="Predicate{T}"/> delegate that defines the conditions of the <see cref="Type"/>s to add as <see cref="Module"/>s. </param>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/>s before they are built. </param>
        /// <returns>
        ///     An <see cref="IReadOnlyList{Module}"/> of all found and added <see cref="Module"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The assembly to add modules from must not be null.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map commands to the root node.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same signature.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.
        /// </exception>
        public IReadOnlyList<Module> AddModules(Assembly assembly, Predicate<TypeInfo> predicate = null, Action<ModuleBuilder> action = null)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly), "The assembly to add modules from must not be null.");

            var modules = ImmutableArray.CreateBuilder<Module>();
            var types = assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i].GetTypeInfo();
                if (!ReflectionUtilities.IsValidModuleDefinition(typeInfo) || typeInfo.IsNested || typeInfo.GetCustomAttribute<DoNotAutomaticallyAddAttribute>() != null)
                    continue;

                if (predicate != null && !predicate(typeInfo))
                    continue;

                modules.Add(AddModule(typeInfo.AsType(), action));
            }

            return modules.TryMoveToImmutable();
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and build a <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="action"> The action to perform on the builder. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"> 
        ///     The module builder action must not be null.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map commands to the root node.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same signature.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.
        /// </exception>
        public Module AddModule(Action<ModuleBuilder> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action), "The action must not be null.");

            var builder = new ModuleBuilder(null);
            action(builder);
            var module = builder.Build(this, null);
            AddModuleInternal(module);
            return module;
        }

        /// <summary>
        ///     Adds the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to add. </param>
        /// <exception cref="ArgumentNullException">
        ///     The module to add must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     This module has already been added.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map commands to the root node.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same signature.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.
        /// </exception>
        public void AddModule(Module module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module), "The module to add must not be null.");

            if (module.Parent != null)
                throw new ArgumentException("The module to add must not be a nested module.", nameof(module));

            AddModuleInternal(module);
        }

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The <see cref="Type"/> to add. </typeparam>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/> before it is built. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     The type has already been added as a module.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map commands to the root node.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same signature.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.
        /// </exception>
        public Module AddModule<TModule>(Action<ModuleBuilder> action = null)
            => AddModule(typeof(TModule), action);

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> to add. </param>
        /// <param name="action"> The optional <see cref="Action{T}"/> delegate that allows for mutation of the <see cref="ModuleBuilder"/> before it is built. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The type to add must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The type has already been added as a module.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map commands to the root node.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same signature.
        /// </exception>
        /// <exception cref="CommandMappingException">
        ///     Cannot map multiple overloads with the same argument types, with one of them being a remainder, if the other one ignores extra arguments.
        /// </exception>
        public Module AddModule(Type type, Action<ModuleBuilder> action = null)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type), "The type to add must not be null.");

            var builder = ReflectionUtilities.CreateModuleBuilder(this, null, type.GetTypeInfo());
            action?.Invoke(builder);
            var module = builder.Build(this, null);
            AddModuleInternal(module);
            return module;
        }

        private void AddModuleInternal(Module module)
        {
            void AddSubmodules(Module m)
            {
                foreach (var submodule in m.Submodules)
                {
                    if (submodule.Type != null)
                        _typeModules.Add(submodule.Type, submodule);

                    AddSubmodules(submodule);
                }
            }

            lock (_moduleLock)
            {
                if (_modules.Contains(module))
                    throw new ArgumentException("This module has already been added.", nameof(module));

                if (module.Type != null && _typeModules.ContainsKey(module.Type))
                    throw new ArgumentException($"{module.Type} has already been added as a module.", nameof(module));

                _map.MapModule(module);
                _modules.Add(module);
                AddSubmodules(module);
            }
        }

        /// <summary>
        ///     Removes the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to remove. </param>
        /// <exception cref="ArgumentNullException">
        ///     The module to remove must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     This module has not been added.
        /// </exception>
        public void RemoveModule(Module module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module), "The module to remove must not be null.");

            RemoveModuleInternal(module);
        }

        /// <summary>
        ///     Removes all added <see cref="Module"/>s.
        /// </summary>
        public void RemoveAllModules()
        {
            foreach (var module in _modules.ToImmutableArray())
                RemoveModuleInternal(module);
        }

        private void RemoveModuleInternal(Module module)
        {
            void RemoveSubmodules(Module m)
            {
                foreach (var submodule in m.Submodules)
                {
                    if (submodule.Type != null)
                        _typeModules.Remove(submodule.Type);

                    RemoveSubmodules(submodule);
                }
            }

            lock (_moduleLock)
            {
                if (!_modules.Contains(module))
                    throw new ArgumentException("This module has not been added.", nameof(module));

                _map.UnmapModule(module);
                _modules.Remove(module);
                if (module.Type != null)
                {
                    _typeModules.Remove(module.Type);
                    RemoveSubmodules(module);
                }
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the input and executes the most suitable one.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(string input, CommandContext context, IServiceProvider provider = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "The input must not be null.");

            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            var matches = FindCommands(input);
            if (matches.Count == 0)
                return new CommandNotFoundResult();

            var failedOverloads = new Dictionary<Command, FailedResult>(matches.Count);
            foreach (var match in matches.GroupBy(x => string.Join(Separator, x.Path)).First())
            {
                context.Command = match.Command;
                context.Alias = match.Alias;
                context.Path = match.Path;
                context.RawArguments = match.RawArguments;

                try
                {
                    var checkResult = await match.Command.RunChecksAsync(context, provider).ConfigureAwait(false);
                    if (checkResult is ChecksFailedResult checksFailedResult)
                    {
                        if (checksFailedResult.Module != null)
                            return checksFailedResult;

                        failedOverloads.Add(match.Command, checksFailedResult);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.Checks, ex);
                    await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                    return executionFailedResult;
                }

                ArgumentParserResult parseResult;
                try
                {
                    parseResult = ArgumentParser.Parse(context);
                    if (!parseResult.IsSuccessful)
                    {
                        failedOverloads.Add(match.Command, new ArgumentParseFailedResult(match.Command, parseResult));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.ArgumentParsing, ex);
                    await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                    return executionFailedResult;
                }

                object[] parsedArguments;
                try
                {
                    var result = await CreateArgumentsAsync(parseResult, context, provider).ConfigureAwait(false);
                    if (result.FailedResult != null)
                    {
                        failedOverloads.Add(match.Command, result.FailedResult);
                        continue;
                    }

                    parsedArguments = result.ParsedArguments;
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.TypeParsing, ex);
                    await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                    return executionFailedResult;
                }

                context.InternalArguments = parsedArguments;
                return await InternalExecuteAsync(context, provider).ConfigureAwait(false);
            }

            return failedOverloads.Count == 1 ? failedOverloads.First().Value : new OverloadsFailedResult(failedOverloads);
        }

        /// <summary>
        ///     Attempts to parse the arguments for the provided <see cref="Command"/> and execute it.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The raw arguments must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(Command command, string rawArguments, CommandContext context, IServiceProvider provider = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "The command must not be null.");

            if (rawArguments == null)
                throw new ArgumentNullException(nameof(rawArguments), "The raw arguments must not be null.");

            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            context.Command = command;
            context.RawArguments = rawArguments;
            try
            {
                var checkResult = await command.RunChecksAsync(context, provider).ConfigureAwait(false);
                if (!checkResult.IsSuccessful)
                    return checkResult;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.Checks, ex);
                await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                return executionFailedResult;
            }

            ArgumentParserResult parseResult;
            try
            {
                parseResult = ArgumentParser.Parse(context);
                if (!parseResult.IsSuccessful)
                    return new ArgumentParseFailedResult(command, parseResult);
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.ArgumentParsing, ex);
                await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                return executionFailedResult;
            }

            object[] parsedArguments;
            try
            {
                var result = await CreateArgumentsAsync(parseResult, context, provider).ConfigureAwait(false);
                if (result.FailedResult != null)
                    return result.FailedResult;

                parsedArguments = result.ParsedArguments;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.TypeParsing, ex);
                await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                return executionFailedResult;
            }

            context.InternalArguments = parsedArguments;
            return await InternalExecuteAsync(context, provider).ConfigureAwait(false);
        }

        /// <summary>
        ///     Attempts to execute the given <see cref="Command"/> with the provided arguments.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="arguments"> The arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The raw arguments must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(Command command, IEnumerable<object> arguments, CommandContext context, IServiceProvider provider = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "The command must not be null.");

            if (arguments is null)
                throw new ArgumentNullException(nameof(arguments), "The arguments must not be null.");

            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            context.Command = command;
            context.InternalArguments = arguments.ToArray();
            try
            {
                var checkResult = await command.RunChecksAsync(context, provider).ConfigureAwait(false);
                if (!checkResult.IsSuccessful)
                    return checkResult;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.Checks, ex);
                await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                return executionFailedResult;
            }

            return await InternalExecuteAsync(context, provider).ConfigureAwait(false);
        }

        private async Task<IResult> InternalExecuteAsync(CommandContext context, IServiceProvider provider)
        {
            async Task<IResult> ExecuteCallbackAsync()
            {
                try
                {
                    var result = await context.Command.Callback(context, provider).ConfigureAwait(false);
                    if (result is ExecutionFailedResult executionFailedResult)
                        await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);

                    else
                    {
                        if (result is CommandResult commandResult)
                            commandResult.Command = context.Command;

                        await InvokeCommandExecutedHandlersAsync(result as CommandResult, context, provider).ConfigureAwait(false);
                    }

                    return result ?? new SuccessfulResult();
                }
                catch (Exception ex)
                {
                    var result = new ExecutionFailedResult(context.Command, CommandExecutionStep.Command, ex);
                    await InvokeCommandErroredHandlersAsync(result, context, provider).ConfigureAwait(false);
                    return result;
                }
            }

            try
            {
                var cooldownResult = context.Command.RunCooldowns(context, provider);
                if (!cooldownResult.IsSuccessful)
                    return cooldownResult;
            }
            catch (Exception ex)
            {
                var result = new ExecutionFailedResult(context.Command, CommandExecutionStep.CooldownBucketKeyGenerating, ex);
                await InvokeCommandErroredHandlersAsync(result, context, provider).ConfigureAwait(false);
                return result;
            }

            switch (context.Command.RunMode)
            {
                case RunMode.Sequential:
                    return await ExecuteCallbackAsync().ConfigureAwait(false);

                case RunMode.Parallel:
                    _ = Task.Run(() => ExecuteCallbackAsync());
                    return new SuccessfulResult();

                default:
                    throw new InvalidOperationException("Invalid run mode.");
            }
        }

        private async Task<(FailedResult FailedResult, object[] ParsedArguments)> CreateArgumentsAsync(ArgumentParserResult parserResult, CommandContext context, IServiceProvider provider = null)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            var parsedArguments = new object[parserResult.Arguments.Count];
            if (parserResult.Arguments.Count == 0)
                return (default, parsedArguments);

            var index = 0;
            for (var i = 0; i < parserResult.Command.Parameters.Count; i++)
            {
                var parameter = parserResult.Command.Parameters[i];
                if (!parserResult.Arguments.TryGetValue(parameter, out var value))
                    throw new InvalidOperationException($"No value for parameter {parameter.Name} ({parameter.Type}) was returned by the argument parser ({ArgumentParser.GetType()}).");

                if (value is IReadOnlyList<string> multipleArguments)
                {
                    var array = Array.CreateInstance(parameter.Type, multipleArguments.Count);
                    for (var j = 0; j < multipleArguments.Count; j++)
                    {
                        var (result, parsedArgument) = await ParseArgumentAsync(parameter, multipleArguments[j], context, provider).ConfigureAwait(false);
                        if (result != null)
                            return (result, default);

                        array.SetValue(parsedArgument, j);
                    }

                    var checkResult = await parameter.RunChecksAsync(array, context, provider).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[index++] = array;
                }

                else
                {
                    var (result, parsedArgument) = await ParseArgumentAsync(parameter, value, context, provider).ConfigureAwait(false);
                    if (result != null)
                        return (result, default);

                    var checkResult = await parameter.RunChecksAsync(parsedArgument, context, provider).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[index++] = parsedArgument;
                }
            }

            return (default, parsedArguments);
        }

        private async Task<(TypeParseFailedResult TypeParseFailedResult, object ParsedArgument)> ParseArgumentAsync(Parameter parameter, object argument, CommandContext context, IServiceProvider provider)
        {
            if (!(argument is string value))
                return (null, argument);

            IPrimitiveTypeParser primitiveParser;
            if (!(parameter.CustomTypeParserType is null))
            {
                var customParser = GetSpecificTypeParser(parameter.Type, parameter.CustomTypeParserType);
                if (customParser is null)
                    throw new InvalidOperationException($"Custom parser of type {parameter.CustomTypeParserType} for parameter {parameter} not found.");

                var typeParserResult = await customParser.ParseAsync(parameter, value, context, provider).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            if (parameter.Type == _stringType)
                return (null, value);

            var parser = GetAnyTypeParser(parameter.Type, (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null);
            if (!(parser is null))
            {
                var typeParserResult = await parser.ParseAsync(parameter, value, context, provider).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            if (primitiveParser == null && (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) == null)
                throw new InvalidOperationException($"No type parser found for parameter {parameter} ({parameter.Type}).");

            if (primitiveParser.TryParse(parameter, value, out var result))
                return (null, result);

            var type = Nullable.GetUnderlyingType(parameter.Type);
            var friendlyName = type == null
                ? CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(parameter.Type, out var name)
                    ? name
                    : parameter.Type.Name
                : CommandUtilities.FriendlyPrimitiveTypeNames.TryGetValue(type, out name)
                    ? $"nullable {name}"
                    : $"nullable {type.Name}";

            return (new TypeParseFailedResult(parameter, value, $"Failed to parse {friendlyName}."), default);
        }

        private async Task InvokeCommandExecutedHandlersAsync(CommandResult result, CommandContext context, IServiceProvider provider)
        {
            var handlers = CommandExecutedHandlers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i](result, context, provider).ConfigureAwait(false);
                }
                catch { }
            }
        }

        private async Task InvokeCommandErroredHandlersAsync(ExecutionFailedResult result, CommandContext context, IServiceProvider provider)
        {
            var handlers = CommandErroredHandlers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i](result, context, provider).ConfigureAwait(false);
                }
                catch { }
            }
        }
    }
}
