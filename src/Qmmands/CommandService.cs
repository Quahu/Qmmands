using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Provides a framework for creating text based commands.
    /// </summary>
    public sealed class CommandService
    {
        /// <summary>
        ///     Gets whether <see cref="FindCommands"/> and primitive <see langword="enum"/> type parsers are case sensitive or not.
        /// </summary>
        public bool CaseSensitive { get; }

        /// <summary>
        ///     Gets the default <see cref="RunMode"/> for commands and modules.
        /// </summary>
        public RunMode DefaultRunMode { get; }

        /// <summary>
        ///     Gets whether commands should ignore extra arguments by default or not.
        /// </summary>
        public bool IgnoreExtraArguments { get; }

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
        ///     Gets the generator to use for <see cref="Cooldown"/> bucket keys.
        /// </summary>
        public ICooldownBucketKeyGenerator CooldownBucketKeyGenerator { get; }

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
        public event Func<Command, CommandResult, ICommandContext, IServiceProvider, Task> CommandExecuted
        {
            add
            {
                lock (_handlerLock)
                    CommandExecutedHandlers = CommandExecutedHandlers.Add(value);
            }
            remove
            {
                lock (_handlerLock)
                    CommandExecutedHandlers = CommandExecutedHandlers.Remove(value);
            }
        }

        /// <summary>
        ///     Gets the <see cref="CommandExecuted"/> event handlers.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImmutableArray<Func<Command, CommandResult, ICommandContext, IServiceProvider, Task>> CommandExecutedHandlers { get; private set; } = ImmutableArray<Func<Command, CommandResult, ICommandContext, IServiceProvider, Task>>.Empty;

        /// <summary>
        ///     Fires when a command fails to execute. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        public event Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task> CommandErrored
        {
            add
            {
                lock (_handlerLock)
                    CommandErroredHandlers = CommandErroredHandlers.Add(value);
            }
            remove
            {
                lock (_handlerLock)
                    CommandErroredHandlers = CommandErroredHandlers.Remove(value);
            }
        }

        /// <summary>
        ///     Gets the <see cref="CommandErroredHandlers"/> event handlers.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImmutableArray<Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task>> CommandErroredHandlers { get; private set; } = ImmutableArray<Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task>>.Empty;

        /// <summary>
        ///     Fires when a non-user instantiated <see cref="ModuleBuilder"/> is about to be built into a <see cref="Module"/>.
        /// </summary>
        public event Func<ModuleBuilder, Task> ModuleBuilding
        {
            add
            {
                lock (_handlerLock)
                    ModuleBuildingHandlers = ModuleBuildingHandlers.Add(value);
            }
            remove
            {
                lock (_handlerLock)
                    ModuleBuildingHandlers = ModuleBuildingHandlers.Add(value);
            }
        }

        /// <summary>
        ///     Gets the <see cref="ModuleBuilding"/> event handlers.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImmutableArray<Func<ModuleBuilder, Task>> ModuleBuildingHandlers { get; private set; } = ImmutableArray<Func<ModuleBuilder, Task>>.Empty;

        internal StringComparison StringComparison { get; }

        private readonly ConcurrentDictionary<Type, Dictionary<Type, (bool ReplacingPrimitive, ITypeParser Instance)>> _parsers;
        private readonly ConcurrentDictionary<Type, IPrimitiveTypeParser> _primitiveParsers;
        private readonly Dictionary<Type, Module> _typeModules;
        private readonly HashSet<Module> _modules;
        private readonly CommandMap _map;
        private static readonly Type _stringType = typeof(string);
        private readonly SemaphoreSlim _moduleSemaphore = new SemaphoreSlim(1, 1);
        private readonly object _handlerLock = new object();

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the specified <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        /// <param name="configuration"> The <see cref="CommandServiceConfiguration"/> to use. </param>
        /// <exception cref="ArgumentNullException">
        ///     The configuration mustn't be null.
        /// </exception>
        public CommandService(CommandServiceConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "The configuration mustn't be null.");

            CaseSensitive = configuration.CaseSensitive;
            DefaultRunMode = configuration.DefaultRunMode;
            IgnoreExtraArguments = configuration.IgnoreExtraArguments;
            Separator = configuration.Separator;
            SeparatorRequirement = configuration.SeparatorRequirement;
            ArgumentParser = configuration.ArgumentParser ?? new DefaultArgumentParser();
            CooldownBucketKeyGenerator = configuration.CooldownBucketKeyGenerator;
            QuotationMarkMap = configuration.QuoteMap != null ? new ReadOnlyDictionary<char, char>(configuration.QuoteMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : CommandUtilities.DefaultQuotationMarkMap;
            NullableNouns = configuration.NullableNouns != null ? configuration.NullableNouns.ToImmutableArray() : CommandUtilities.DefaultNullableNouns;

            StringComparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            _typeModules = new Dictionary<Type, Module>();
            _modules = new HashSet<Module>();
            _map = new CommandMap(this);
            _parsers = new ConcurrentDictionary<Type, Dictionary<Type, (bool, ITypeParser)>>();
            _primitiveParsers = new ConcurrentDictionary<Type, IPrimitiveTypeParser>();
            foreach (var type in ReflectionUtilities.TryParseDelegates.Keys)
            {
                var primitiveTypeParser = ReflectionUtilities.CreatePrimitiveTypeParser(type);
                _primitiveParsers.TryAdd(type, primitiveTypeParser);
                _primitiveParsers.TryAdd(ReflectionUtilities.MakeNullable(type), ReflectionUtilities.CreateNullablePrimitiveTypeParser(type, primitiveTypeParser));
            }
        }

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the default <see cref="CommandServiceConfiguration"/>
        /// </summary>
        public CommandService() : this(CommandServiceConfiguration.Default)
        { }

        /// <summary>
        ///     Enumerates through all of the added <see cref="Module"/>s and yields all found <see cref="Command"/>s.
        /// </summary>
        /// <returns>
        ///     An enumerable with all <see cref="Command"/>s.
        /// </returns>
        public IEnumerable<Command> GetAllCommands()
        {
            IEnumerable<Command> GetCommands(Module module)
            {
                for (var i = 0; i < module.Commands.Count; i++)
                    yield return module.Commands[i];

                for (var i = 0; i < module.Submodules.Count; i++)
                    foreach (var command in GetCommands(module.Submodules[i]))
                        yield return command;
            }

            foreach (var module in _modules.ToImmutableArray())
                foreach (var command in GetCommands(module))
                    yield return command;
        }

        /// <summary>
        ///     Enumerates through all of the added <see cref="Module"/>s yields them.
        /// </summary>
        /// <returns>
        ///     An enumerable with all <see cref="Module"/>s.
        /// </returns>
        public IEnumerable<Module> GetAllModules()
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

            foreach (var module in _modules.ToImmutableArray())
            {
                yield return module;

                foreach (var submodule in GetSubmodules(module))
                    yield return submodule;
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     An ordered enumerable of <see cref="CommandMatch"/>es.
        /// </returns>
        public IEnumerable<CommandMatch> FindCommands(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path to find commands for mustn't be null.");

            return _map.FindCommands(path).OrderByDescending(x => x.Path.Count)
                .ThenByDescending(x => x.Command.Priority)
                .ThenByDescending(x => x.Command.Parameters.Count);
        }

        /// <summary>
        ///     Attempts to find <see cref="Module"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns>
        ///     An ordered enumerable of <see cref="ModuleMatch"/>es.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The path to find modules for mustn't be null.
        /// </exception>
        public IEnumerable<ModuleMatch> FindModules(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path to find modules for mustn't be null.");

            return _map.FindModules(path).OrderByDescending(x => x.Path.Count);
        }

        /// <summary>
        ///     Adds a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The type to add the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to add for the <see cref="Type"/>. </param>
        /// <param name="replacePrimitive"> Whether to replace the primitive parser. </param>
        /// <exception cref="ArgumentNullException">
        ///     The type parser to add mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Custom enum type parsers aren't supported.
        /// </exception>
        public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to add mustn't be null.");

            var type = typeof(T);
            if (ReflectionUtilities.IsNullable(type))
                throw new ArgumentException("Cannot add custom nullable type parsers.", nameof(T));

            AddParserInternal(type, parser, replacePrimitive);
        }

        private void AddParserInternal(Type type, ITypeParser parser, bool replacePrimitive = false)
        {
            _parsers.AddOrUpdate(type,
            new Dictionary<Type, (bool, ITypeParser)> { [parser.GetType()] = (replacePrimitive, parser) },
            (_, v) =>
            {
                v.Add(parser.GetType(), (replacePrimitive, parser));
                return v;
            });

            if (type.IsValueType)
            {
                var nullableParser = ReflectionUtilities.CreateNullableTypeParser(type, this, parser);
                _parsers.AddOrUpdate(ReflectionUtilities.MakeNullable(type),
                    new Dictionary<Type, (bool, ITypeParser)> { [nullableParser.GetType()] = (replacePrimitive, nullableParser) },
                    (_, v) =>
                    {
                        v.Add(nullableParser.GetType(), (replacePrimitive, nullableParser));
                        return v;
                    });
            }
        }

        /// <summary>
        ///     Removes a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> to remove the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to remove for the <see cref="Type"/>. </param>
        /// <exception cref="ArgumentNullException">
        ///     The type parser to remove mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     A parser for this type hasn't been added.
        /// </exception>
        public void RemoveTypeParser<T>(TypeParser<T> parser)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to remove mustn't be null.");

            var type = typeof(T);
            if (!_parsers.ContainsKey(type))
                throw new ArgumentException($"A type parser for type {type} hasn't been added.", nameof(T));

            RemoveParserInternal(type, parser);
        }

        private void RemoveParserInternal(Type type, ITypeParser parser)
        {
            if (!_parsers.TryGetValue(type, out var typeParsers))
                return;

            typeParsers.Remove(parser.GetType());

            if (type.IsValueType)
                typeParsers.Remove(ReflectionUtilities.MakeNullable(type));
        }

        internal ITypeParser GetSpecificTypeParser(Type type, Type parserType)
            => _parsers.TryGetValue(type, out var typeParsers) && typeParsers.TryGetValue(parserType, out var typeParser) ? typeParser.Instance : null;

        internal ITypeParser GetAnyTypeParser(Type type, bool replacingPrimitive)
        {
            if (_parsers.TryGetValue(type, out var typeParsers))
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
            if (_primitiveParsers.TryGetValue(type, out var typeParser))
                return typeParser;

            if (type.IsEnum)
            {
                var enumParser = ReflectionUtilities.CreateEnumTypeParser(type.GetEnumUnderlyingType(), type, !CaseSensitive);
                _primitiveParsers.TryAdd(type, enumParser);
                _primitiveParsers.TryAdd(ReflectionUtilities.MakeNullable(type), ReflectionUtilities.CreateNullableEnumTypeParser(type.GetEnumUnderlyingType(), enumParser));
                return enumParser;
            }

            if (ReflectionUtilities.IsNullable(type) && (type = Nullable.GetUnderlyingType(type)).IsEnum)
                return GetPrimitiveTypeParser(type);

            return null;
        }

        /// <summary>
        ///     Attempts to add all valid <see cref="Module"/>s and <see cref="Command"/>s found in the provided <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly"> The assembly to search. </param>
        /// <returns>
        ///     An <see cref="IReadOnlyList{Module}"/> of all found and added <see cref="Module"/>s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The assembly to add modules from mustn't be null.
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
        public async Task<IReadOnlyList<Module>> AddModulesAsync(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly), "The assembly to add modules from mustn't be null.");

            var modules = new List<Module>();
            var types = assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i].GetTypeInfo();
                if (!ReflectionUtilities.IsValidModuleDefinition(typeInfo) || typeInfo.IsNested || typeInfo.GetCustomAttribute<DontAutoAddAttribute>() != null)
                    continue;

                modules.Add(await AddModuleAsync(typeInfo.AsType()).ConfigureAwait(false));
            }

            return modules.AsReadOnly();
        }

        /// <summary>
        ///     Attempts to build the specified <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builder"> The builder to build. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The module builder to add mustn't be null.
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
        public async Task<Module> AddModuleAsync(ModuleBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder), "The module builder to add mustn't be null.");

            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                var module = builder.Build(this, null);
                AddModuleInternal(module);
                return module;
            }
            finally
            {
                _moduleSemaphore.Release();
            }
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and build a <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        /// <returns>
        ///     A <see cref="Module"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"> 
        ///     The module builder action mustn't be null.
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
        public Task<Module> AddModuleAsync(Action<ModuleBuilder> builderAction)
        {
            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction), "The module builder action mustn't be null.");

            var builder = new ModuleBuilder();
            builderAction(builder);
            return AddModuleAsync(builder);
        }

        /// <summary>
        ///     Adds the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to add. </param>
        /// <exception cref="ArgumentNullException">
        ///     The module to add mustn't be null.
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
        public async Task AddModuleAsync(Module module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module), "The module to add mustn't be null.");

            if (module.Parent != null)
                throw new ArgumentException("The module to add mustn't be a nested module.", nameof(module));

            if (_modules.Contains(module))
                throw new ArgumentException("This module has already been added.", nameof(module));

            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                AddModuleInternal(module);
            }
            finally
            {
                _moduleSemaphore.Release();
            }
        }

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The <see cref="Type"/> to add. </typeparam>
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
        public Task<Module> AddModuleAsync<TModule>()
            => AddModuleAsync(typeof(TModule));

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <returns> A <see cref="Module"/>. </returns>
        /// <exception cref="ArgumentNullException">
        ///     The type to add mustn't be null.
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
        public async Task<Module> AddModuleAsync(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type), "The type to add mustn't be null.");

            if (_typeModules.ContainsKey(type))
                throw new ArgumentException($"{type} has already been added as a module.", nameof(type));

            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                var moduleBuilder = ReflectionUtilities.BuildModule(type.GetTypeInfo());
                await InvokeModuleBuildingHandlersAsync(moduleBuilder).ConfigureAwait(false);
                var module = moduleBuilder.Build(this, null);
                AddModuleInternal(module);
                return module;
            }
            finally
            {
                _moduleSemaphore.Release();
            }
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

            _map.MapModule(module, new List<string>());
            _modules.Add(module);
            AddSubmodules(module);
        }

        /// <summary>
        ///     Removes the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"> The <see cref="Module"/> to remove. </param>
        /// <exception cref="ArgumentNullException">
        ///     The module to remove mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     This module hasn't been added.
        /// </exception>
        public async Task RemoveModuleAsync(Module module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module), "The module to remove mustn't be null.");

            if (!_modules.Contains(module))
                throw new ArgumentException("This module hasn't been added.", nameof(module));

            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                RemoveModuleInternal(module);
            }
            finally
            {
                _moduleSemaphore.Release();
            }
        }

        /// <summary>
        ///     Removes all added <see cref="Module"/>s.
        /// </summary>
        public async Task RemoveAllModulesAsync()
        {
            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                foreach (var module in _modules.ToImmutableArray())
                    RemoveModuleInternal(module);
            }
            finally
            {
                _moduleSemaphore.Release();
            }
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

            _map.UnmapModule(module, new List<string>());
            _modules.Remove(module);
            if (module.Type != null)
            {
                _typeModules.Remove(module.Type);
                RemoveSubmodules(module);
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the input and executes the most suitable one.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns> An <see cref="IResult"/>. </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context mustn't be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(string input, ICommandContext context, IServiceProvider provider = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "The input mustn't be null.");

            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context mustn't be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            var matches = FindCommands(input).ToImmutableArray();
            if (matches.Length == 0)
                return new CommandNotFoundResult();

            var failedOverloads = new Dictionary<Command, FailedResult>();
            foreach (var match in matches.GroupBy(x => string.Join(Separator, x.Path)).First())
            {
                try
                {
                    var checkResult = await match.Command.RunChecksAsync(context, provider).ConfigureAwait(false);
                    if (checkResult is ChecksFailedResult checksFailedResult)
                    {
                        if (checksFailedResult.Module != null)
                            return checksFailedResult;

                        failedOverloads.Add(match.Command, checkResult as FailedResult);
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
                    parseResult = ArgumentParser.ParseRawArguments(match.Command, match.RawArguments);
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

                object[] parsedArguments = null;
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

                var cooldownResult = match.Command.RunCooldowns(context, provider);
                if (!cooldownResult.IsSuccessful)
                    return cooldownResult;

                switch (match.Command.RunMode)
                {
                    case RunMode.Sequential:
                        return await ExecuteInternalAsync(match.Command, context, provider, parsedArguments).ConfigureAwait(false);

                    case RunMode.Parallel:
                        _ = Task.Run(() => ExecuteInternalAsync(match.Command, context, provider, parsedArguments));
                        return new SuccessfulResult();

                    default:
                        throw new InvalidOperationException("Invalid run mode.");
                }
            }

            return failedOverloads.Count == 1 ? failedOverloads.First().Value : new OverloadsFailedResult(failedOverloads);
        }

        /// <summary>
        ///     Attempts to parse the arguments for the provided <see cref="Command"/> and execute it.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns> An <see cref="IResult"/>. </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The raw arguments mustn't be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context mustn't be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(Command command, string rawArguments, ICommandContext context, IServiceProvider provider = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "The command mustn't be null.");

            if (rawArguments == null)
                throw new ArgumentNullException(nameof(rawArguments), "The input mustn't be null.");

            if (context is null)
                throw new ArgumentNullException(nameof(context), "The context mustn't be null.");

            if (provider is null)
                provider = DummyServiceProvider.Instance;

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
                parseResult = ArgumentParser.ParseRawArguments(command, rawArguments);
                if (!parseResult.IsSuccessful)
                    return new ArgumentParseFailedResult(command, parseResult);
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.ArgumentParsing, ex);
                await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);
                return executionFailedResult;
            }

            object[] parsedArguments = null;
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

            var cooldownResult = command.RunCooldowns(context, provider);
            if (!cooldownResult.IsSuccessful)
                return cooldownResult;

            switch (command.RunMode)
            {
                case RunMode.Sequential:
                    return await ExecuteInternalAsync(command, context, provider, parsedArguments).ConfigureAwait(false);

                case RunMode.Parallel:
                    _ = Task.Run(() => ExecuteInternalAsync(command, context, provider, parsedArguments));
                    return new SuccessfulResult();

                default:
                    throw new InvalidOperationException("Invalid run mode.");
            }
        }

        private async Task<(FailedResult FailedResult, object[] ParsedArguments)> CreateArgumentsAsync(ArgumentParserResult parseResult, ICommandContext context, IServiceProvider provider)
        {
            var parsedArguments = new object[parseResult.Arguments.Count];
            if (parseResult.Arguments.Count == 0)
                return (default, parsedArguments);

            var index = 0;
            foreach (var kvp in parseResult.Arguments)
            {
                var parameter = kvp.Key;
                if (kvp.Value is IReadOnlyList<string> multipleArguments)
                {
                    var array = Array.CreateInstance(parameter.Type, multipleArguments.Count);
                    for (var i = 0; i < multipleArguments.Count; i++)
                    {
                        var (result, parsed) = await ParseArgumentAsync(parameter, multipleArguments[i], context, provider).ConfigureAwait(false);
                        if (result != null)
                            return (result, default);

                        array.SetValue(parsed, i);
                    }

                    var checkResult = await parameter.RunChecksAsync(array, context, provider).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[index++] = array;
                }

                else
                {
                    var (result, parsed) = await ParseArgumentAsync(parameter, kvp.Value, context, provider).ConfigureAwait(false);
                    if (result != null)
                        return (result, default);

                    var checkResult = await parameter.RunChecksAsync(parsed, context, provider).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[index++] = parsed;
                }
            }

            return (default, parsedArguments);
        }

        private async Task<(FailedResult FailedResult, object Parsed)> ParseArgumentAsync(Parameter parameter, object argument, ICommandContext context, IServiceProvider provider)
        {
            if (!(argument is string value))
                return (null, argument);

            if (parameter.Type == _stringType)
                return (null, value);

            IPrimitiveTypeParser primitiveParser;
            if (!(parameter.CustomTypeParserType is null))
            {
                var customParser = GetSpecificTypeParser(parameter.Type, parameter.CustomTypeParserType);
                if (customParser is null)
                    throw new InvalidOperationException($"Custom parser of type {parameter.CustomTypeParserType.Name} for parameter {parameter.Name} not found.");

                var typeParserResult = await customParser.ParseAsync(value, context, provider).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            var parser = GetAnyTypeParser(parameter.Type, (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null);
            if (!(parser is null))
            {
                var typeParserResult = await parser.ParseAsync(value, context, provider).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            if (primitiveParser == null && (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) == null)
                throw new InvalidOperationException($"No type parser found for parameter {parameter} ({parameter.Type}).");

            if (primitiveParser.TryParse(this, value, out var result))
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

        private async Task<IResult> ExecuteInternalAsync(Command command, ICommandContext context, IServiceProvider provider, object[] arguments)
        {
            try
            {
                var result = await command.Callback(command, arguments, context, provider).ConfigureAwait(false);
                if (result is ExecutionFailedResult executionFailedResult)
                    await InvokeCommandErroredHandlersAsync(executionFailedResult, context, provider).ConfigureAwait(false);

                else
                {
                    if (result is CommandResult commandResult)
                        commandResult.Command = command;

                    await InvokeCommandExecutedHandlersAsync(command, result as CommandResult, context, provider).ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception ex)
            {
                var result = new ExecutionFailedResult(command, CommandExecutionStep.Command, ex);
                await InvokeCommandErroredHandlersAsync(result, context, provider).ConfigureAwait(false);
                return result;
            }
        }

        private async Task InvokeCommandExecutedHandlersAsync(Command command, CommandResult result, ICommandContext context, IServiceProvider provider)
        {
            var handlers = CommandExecutedHandlers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i].Invoke(command, result, context, provider).ConfigureAwait(false);
                }
                catch { }
            }
        }

        private async Task InvokeCommandErroredHandlersAsync(ExecutionFailedResult result, ICommandContext context, IServiceProvider provider)
        {
            var handlers = CommandErroredHandlers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i].Invoke(result, context, provider).ConfigureAwait(false);
                }
                catch { }
            }
        }

        private async Task InvokeModuleBuildingHandlersAsync(ModuleBuilder builder)
        {
            var handlers = ModuleBuildingHandlers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i].Invoke(builder).ConfigureAwait(false);
                }
                catch { }
            }
        }
    }
}
