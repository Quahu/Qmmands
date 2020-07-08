using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands.Delegates;
using Qommon.Collections;
using Qommon.Events;

namespace Qmmands
{
    /// <summary>
    ///     Provides a framework for handling text based commands.
    /// </summary>
    public class CommandService : ICommandService
    {
        /// <summary>
        ///     Gets the <see cref="System.StringComparison"/> used for finding <see cref="Command"/>s and <see cref="Module"/>s,
        ///     used by the default <see langword="enum"/> parsers, and comparing <see cref="NullableNouns"/>.
        /// </summary>
        public StringComparison StringComparison { get; }

        /// <summary>
        ///     Gets the default <see cref="RunMode"/> for commands and modules.
        /// </summary>
        public RunMode DefaultRunMode { get; }

        /// <summary>
        ///     Gets whether <see cref="Command"/>s ignore extra arguments by default or not.
        /// </summary>
        public bool IgnoresExtraArguments { get; }

        /// <summary>
        ///     Gets the <see cref="string"/> separator used between groups and commands.
        /// </summary>
        public string Separator { get; }

        /// <summary>
        ///     Gets the separator requirement.
        /// </summary>
        public SeparatorRequirement SeparatorRequirement { get; }

        /// <summary>
        ///     Gets the default <see cref="IArgumentParser"/>.
        /// </summary>
        public IArgumentParser DefaultArgumentParser { get; private set; }

        /// <summary>
        ///     Gets the generator <see langword="delegate"/> used for <see cref="Cooldown"/> bucket keys.
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
        ///     Gets the top-level modules.
        /// </summary>
        public ReadOnlySet<Module> TopLevelModules { get; }

        /// <summary>
        ///     Fires after a <see cref="Command"/> was successfully executed.
        ///     You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        public event AsynchronousEventHandler<CommandExecutedEventArgs> CommandExecuted
        {
            add => _commandExecuted.Hook(value);
            remove => _commandExecuted.Unhook(value);
        }
        private readonly AsynchronousEvent<CommandExecutedEventArgs> _commandExecuted = new AsynchronousEvent<CommandExecutedEventArgs>();

        /// <summary>
        ///     Fires after a <see cref="Command"/> failed to execute.
        ///     You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        public event AsynchronousEventHandler<CommandExecutionFailedEventArgs> CommandExecutionFailed
        {
            add => _commandExecutionFailed.Hook(value);
            remove => _commandExecutionFailed.Unhook(value);
        }
        private readonly AsynchronousEvent<CommandExecutionFailedEventArgs> _commandExecutionFailed = new AsynchronousEvent<CommandExecutionFailedEventArgs>();

        internal readonly StringComparer StringComparer;

        private readonly ConcurrentDictionary<Type, Dictionary<Type, (bool ReplacingPrimitive, ITypeParser Instance)>> _typeParsers;
        private readonly ConcurrentDictionary<Type, IPrimitiveTypeParser> _primitiveTypeParsers;
        private readonly HashSet<Module> _topLevelModules;
        private readonly CommandMap _map;
        private readonly ConcurrentDictionary<Type, IArgumentParser> _argumentParsers;
        private static readonly Type _stringType = typeof(string);
        private readonly object _moduleLock = new object();

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

            StringComparison = configuration.StringComparison;
            DefaultRunMode = configuration.DefaultRunMode;
            IgnoresExtraArguments = configuration.IgnoresExtraArguments;
            Separator = configuration.Separator;
            SeparatorRequirement = configuration.SeparatorRequirement;
            CooldownBucketKeyGenerator = configuration.CooldownBucketKeyGenerator;
            QuotationMarkMap = configuration.QuoteMap != null
                ? new ReadOnlyDictionary<char, char>(configuration.QuoteMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                : CommandUtilities.DefaultQuotationMarkMap;
            NullableNouns = configuration.NullableNouns != null
                ? configuration.NullableNouns.ToImmutableArray()
                : CommandUtilities.DefaultNullableNouns;

            StringComparer = StringComparer.FromComparison(StringComparison);

            _topLevelModules = new HashSet<Module>();
            TopLevelModules = new ReadOnlySet<Module>(_topLevelModules);
            _map = new CommandMap(this);
            _argumentParsers = new ConcurrentDictionary<Type, IArgumentParser>(Environment.ProcessorCount, 1);
            SetDefaultArgumentParser(configuration.DefaultArgumentParser ?? Qmmands.DefaultArgumentParser.Instance);
            _typeParsers = new ConcurrentDictionary<Type, Dictionary<Type, (bool, ITypeParser)>>();
            _primitiveTypeParsers = new ConcurrentDictionary<Type, IPrimitiveTypeParser>(Environment.ProcessorCount, CommandUtilities.PrimitiveTypeParserCount * 2);
            foreach (var type in Utilities.TryParseDelegates.Keys)
            {
                var primitiveTypeParser = Utilities.CreatePrimitiveTypeParser(type);
                _primitiveTypeParsers.TryAdd(type, primitiveTypeParser);
                _primitiveTypeParsers.TryAdd(Utilities.MakeNullable(type), Utilities.CreateNullablePrimitiveTypeParser(type, primitiveTypeParser));
            }
        }

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the default <see cref="CommandServiceConfiguration"/>.
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
            static IEnumerable<Command> GetCommands(Module module)
            {
                for (var i = 0; i < module.Commands.Count; i++)
                    yield return module.Commands[i];

                for (var i = 0; i < module.Submodules.Count; i++)
                {
                    foreach (var command in GetCommands(module.Submodules[i]))
                        yield return command;
                }
            }

            lock (_moduleLock)
            {
                return _topLevelModules.SelectMany(GetCommands).ToImmutableArray();
            }
        }

        /// <summary>
        ///     Gets all of the added <see cref="Module"/>s.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Module"/>s.
        /// </returns>
        public IReadOnlyList<Module> GetAllModules()
        {
            static IEnumerable<Module> GetSubmodules(Module module)
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
                foreach (var module in _topLevelModules)
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
        /// <exception cref="ArgumentNullException">
        ///    The path must not be null.
        /// </exception>
        public IReadOnlyList<CommandMatch> FindCommands(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path to find commands for must not be null.");

            List<CommandMatch> list;
            lock (_moduleLock)
            {
                var matches = _map.FindCommands(path);
                if (matches.Count == 0)
                    return matches;

                // TODO nuke this if custom maps are a thing
                list = matches as List<CommandMatch>;
            }

            list.Sort(Utilities.CommandOverloadComparer.Instance);
            return new ReadOnlyList<CommandMatch>(list);
        }

        /// <summary>
        ///     Sets an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/> as the default parser.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        /// <exception cref="ArgumentException">
        ///     An argument parser of this type has not been added.
        /// </exception>
        public void SetDefaultArgumentParser<T>() where T : IArgumentParser
            => SetDefaultArgumentParser(typeof(T));

        /// <summary>
        ///     Sets an <see cref="IArgumentParser"/> of the specified <see cref="Type"/> as the default parser.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        /// <exception cref="ArgumentNullException">
        ///     The argument parser type to set must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     An argument parser of this type has not been added.
        /// </exception>
        public void SetDefaultArgumentParser(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "The argument parser type to set must not be null.");

            var parser = GetArgumentParser(type);
            if (parser == null)
                throw new ArgumentException("An argument parser of this type has not been added.", nameof(type));

            DefaultArgumentParser = parser;
        }

        /// <summary>
        ///     Sets and adds, if it has not been added before, an <see cref="IArgumentParser"/> as the default parser.
        /// </summary>
        /// <param name="parser"> The <see cref="IArgumentParser"/> to set. </param>
        /// <exception cref="ArgumentNullException">
        ///     The argument parser to set must not be null.
        /// </exception>
        public void SetDefaultArgumentParser(IArgumentParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser), "The argument parser to add must not be null.");

            _argumentParsers.TryAdd(parser.GetType(), parser);
            DefaultArgumentParser = parser;
        }

        /// <summary>
        ///     Adds an <see cref="IArgumentParser"/>.
        /// </summary>
        /// <param name="parser"> The <see cref="IArgumentParser"/> to add. </param>
        /// <exception cref="ArgumentNullException">
        ///     The argument parser to add must not be null.
        /// </exception>
        public void AddArgumentParser(IArgumentParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser), "The argument parser to add must not be null.");

            if (!_argumentParsers.TryAdd(parser.GetType(), parser))
                throw new ArgumentException("This argument parser has already been added.", nameof(parser));
        }

        /// <summary>
        ///     Removes an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        public void RemoveArgumentParser<T>() where T : IArgumentParser
            => RemoveArgumentParser(typeof(T));

        /// <summary>
        ///     Removes an <see cref="IArgumentParser"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        /// <exception cref="ArgumentNullException">
        ///     The argument parser type to remove must not be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The argument parser type to remove must not be the default argument parser's type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     This argument parser type has not been added.
        /// </exception>
        public void RemoveArgumentParser(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "The argument parser type to remove must not be null.");

            if (DefaultArgumentParser.GetType() == type)
                throw new ArgumentException("The argument parser type to remove must not be the default argument parser's type.", nameof(type));

            if (!_argumentParsers.TryRemove(type, out _))
                throw new ArgumentException("This argument parser type has not been added.", nameof(type));
        }

        /// <summary>
        ///     Gets an <see cref="IArgumentParser"/> of the specified <typeparamref name="T"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </typeparam>
        /// <returns>
        ///     The <see cref="IArgumentParser"/> or <see langword="null"/>.
        /// </returns>
        public IArgumentParser GetArgumentParser<T>() where T : IArgumentParser
            => GetArgumentParser(typeof(T));

        /// <summary>
        ///     Gets an <see cref="IArgumentParser"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="IArgumentParser"/>. </param>
        /// <returns>
        ///     The <see cref="IArgumentParser"/> or <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The argument parser type to get must not be null.
        /// </exception>
        public IArgumentParser GetArgumentParser(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "The argument parser type to get must not be null.");

            return _argumentParsers.TryGetValue(type, out var parser)
                  ? parser
                  : null;
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
        public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser), "The type parser to add must not be null.");

            var type = typeof(T);
            if (Utilities.IsNullable(type))
                throw new ArgumentException("Cannot add custom type parsers for nullable types.", nameof(T));

            if (replacePrimitive)
            {
                if (GetPrimitiveTypeParser(type) == null)
                    throw new ArgumentException($"No primitive type parser found to replace for type {type}.", nameof(T));

                var existingParser = GetAnyTypeParser(type, true);
                if (existingParser != null)
                    throw new ArgumentException($"There is already a custom type parser replacing the primitive parser for type {type} - {existingParser.GetType()}.");
            }

            AddTypeParserInternal(type, parser, replacePrimitive);
        }

        internal void AddTypeParserInternal(Type type, ITypeParser parser, bool replacePrimitive)
        {
            var parserType = parser.GetType();
            _typeParsers.AddOrUpdate(type,
                _ => new Dictionary<Type, (bool, ITypeParser)> { [parserType] = (replacePrimitive, parser) },
                (_, v) =>
                {
                    lock (v)
                    {
                        v.Add(parserType, (replacePrimitive, parser));
                        return v;
                    }
                });

            if (type.IsValueType)
            {
                var nullableParser = Utilities.CreateNullableTypeParser(type, parser);
                _typeParsers.AddOrUpdate(Utilities.MakeNullable(type),
                    _ => new Dictionary<Type, (bool, ITypeParser)> { [parserType] = (replacePrimitive, nullableParser) },
                    (_, v) =>
                    {
                        lock (v)
                        {
                            v.Add(parserType, (replacePrimitive, nullableParser));
                            return v;
                        }
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
            if (parser == null)
                throw new ArgumentNullException(nameof(parser), "The type parser to remove must not be null.");

            var type = typeof(T);
            var parserType = parser.GetType();
            bool found;
            if (_typeParsers.TryGetValue(type, out var typeParsers))
            {
                lock (typeParsers)
                {
                    found = typeParsers.Remove(parserType);
                }
            }
            else
            {
                found = false;
            }

            if (!found)
                throw new ArgumentException("The type parser to remove has not been added.");

            if (type.IsValueType)
            {
                var nullableType = Utilities.MakeNullable(type);
                if (_typeParsers.TryGetValue(nullableType, out var nullableTypeParsers))
                {
                    lock (nullableTypeParsers)
                    {
                        nullableTypeParsers.Remove(parserType);
                    }
                }
            }
        }

        /// <summary>
        ///     Removes all added <see cref="TypeParser{T}"/>s. This does not affect primitive type parsers.
        /// </summary>
        public void RemoveAllTypeParsers()
            => _typeParsers.Clear();

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
        {
            if (!_typeParsers.TryGetValue(type, out var typeParsers))
                return null;

            lock (typeParsers)
            {
                return typeParsers.TryGetValue(parserType, out var typeParser)
                    ? typeParser.Instance
                    : null;
            }
        }

        internal ITypeParser GetAnyTypeParser(Type type, bool replacingPrimitive)
        {
            if (_typeParsers.TryGetValue(type, out var typeParsers))
            {
                lock (typeParsers)
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
                    {
                        return typeParsers.First().Value.Instance;
                    }
                }
            }

            return null;
        }

        internal IPrimitiveTypeParser GetPrimitiveTypeParser(Type type)
        {
            if (_primitiveTypeParsers.TryGetValue(type, out var typeParser))
                return typeParser;

            if (type.IsEnum)
            {
                var enumParser = Utilities.CreateEnumTypeParser(type.GetEnumUnderlyingType(), type, this);
                _primitiveTypeParsers.TryAdd(type, enumParser);
                _primitiveTypeParsers.TryAdd(Utilities.MakeNullable(type), Utilities.CreateNullableEnumTypeParser(type.GetEnumUnderlyingType(), enumParser));
                return enumParser;
            }

            return Utilities.IsNullable(type) && (type = Nullable.GetUnderlyingType(type)).IsEnum
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
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), "The assembly to add modules from must not be null.");

            var modules = ImmutableArray.CreateBuilder<Module>();
            var types = assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i].GetTypeInfo();
                if (!Utilities.IsValidModuleDefinition(typeInfo) || typeInfo.IsNested || typeInfo.GetCustomAttribute<DoNotAddAttribute>() != null)
                    continue;

                if (predicate != null && !predicate(typeInfo))
                    continue;

                try
                {
                    modules.Add(AddModule(typeInfo.AsType(), action));
                }
                catch
                {
                    for (var j = 0; j < modules.Count; j++)
                        RemoveModule(modules[j]);

                    throw;
                }
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

            lock (_moduleLock)
            {
                if (_topLevelModules.Contains(module))
                    throw new ArgumentException("This module has already been added.", nameof(module));

                AddModuleInternal(module);
            }
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
            if (type == null)
                throw new ArgumentNullException(nameof(type), "The type to add must not be null.");

            var builder = Utilities.CreateModuleBuilder(this, null, type.GetTypeInfo());
            action?.Invoke(builder);
            var module = builder.Build(this, null);
            AddModuleInternal(module);
            return module;
        }

        private void AddModuleInternal(Module module)
        {
            lock (_moduleLock)
            {
                _map.MapModule(module);
                _topLevelModules.Add(module);
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
            Module[] topLevelModules;
            lock (_moduleLock)
            {
                topLevelModules = _topLevelModules.ToArray();
                for (var i = 0; i < topLevelModules.Length; i++)
                    RemoveModuleInternal(topLevelModules[i]);
            }
        }

        private void RemoveModuleInternal(Module module)
        {
            lock (_moduleLock)
            {
                if (!_topLevelModules.Remove(module))
                    throw new ArgumentException("This module has not been added.", nameof(module));

                _map.UnmapModule(module);
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the input and executes the most suitable one.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The input must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public async Task<IResult> ExecuteAsync(string input, CommandContext context)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "The input must not be null.");

            if (context == null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            var matches = FindCommands(input);
            if (matches.Count == 0)
                return new CommandNotFoundResult();

            var pathLength = matches[0].Path.Count;
            Dictionary<Command, FailedResult> failedOverloads = null;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Path.Count < pathLength)
                    continue;

                if (!match.Command.IsEnabled)
                {
                    AddFailedOverload(ref failedOverloads, match.Command, new CommandDisabledResult(match.Command));
                    continue;
                }

                context.Command = match.Command;
                context.Alias = match.Alias;
                context.Path = match.Path;
                context.RawArguments = match.RawArguments;

                try
                {
                    var checkResult = await match.Command.RunChecksAsync(context).ConfigureAwait(false);
                    if (checkResult is ChecksFailedResult checksFailedResult)
                    {
                        if (checksFailedResult.Module != null || matches.Count == 1)
                            return checksFailedResult;

                        AddFailedOverload(ref failedOverloads, match.Command, checksFailedResult);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.Checks, ex);
                    await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                    return executionFailedResult;
                }

                ArgumentParserResult argumentParserResult;
                try
                {
                    IArgumentParser argumentParser;
                    if (match.Command.CustomArgumentParserType != null)
                    {
                        argumentParser = GetArgumentParser(match.Command.CustomArgumentParserType);
                        if (argumentParser == null)
                            throw new InvalidOperationException($"Custom argument parser of type {match.Command.CustomArgumentParserType} for command {match.Command} not found.");
                    }
                    else
                    {
                        argumentParser = DefaultArgumentParser;
                    }

                    argumentParserResult = await argumentParser.ParseAsync(context).ConfigureAwait(false);
                    if (argumentParserResult == null)
                        throw new InvalidOperationException("The result from IArgumentParser.ParseAsync must not be null.");

                    if (!argumentParserResult.IsSuccessful)
                    {
                        if (matches.Count == 1)
                            return new ArgumentParseFailedResult(context, argumentParserResult);

                        AddFailedOverload(ref failedOverloads, match.Command, new ArgumentParseFailedResult(context, argumentParserResult));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.ArgumentParsing, ex);
                    await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                    return executionFailedResult;
                }

                object[] parsedArguments;
                try
                {
                    var result = await CreateArgumentsAsync(argumentParserResult, context).ConfigureAwait(false);
                    if (result.FailedResult != null)
                    {
                        if (matches.Count == 1)
                            return result.FailedResult;

                        AddFailedOverload(ref failedOverloads, match.Command, result.FailedResult);
                        continue;
                    }

                    parsedArguments = result.ParsedArguments;
                }
                catch (Exception ex)
                {
                    var executionFailedResult = new ExecutionFailedResult(match.Command, CommandExecutionStep.TypeParsing, ex);
                    await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                    return executionFailedResult;
                }

                context.InternalArguments = parsedArguments;
                return await InternalExecuteAsync(context).ConfigureAwait(false);
            }

            return new OverloadsFailedResult(failedOverloads);
        }

        private static void AddFailedOverload(ref Dictionary<Command, FailedResult> failedOverloads,
            Command command, FailedResult result)
        {
            if (failedOverloads == null)
                failedOverloads = new Dictionary<Command, FailedResult>(4);

            failedOverloads[command] = result;
        }

        /// <summary>
        ///     Attempts to parse the arguments for the provided <see cref="Command"/> and execute it.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
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
        public async Task<IResult> ExecuteAsync(Command command, string rawArguments, CommandContext context)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "The command must not be null.");

            if (rawArguments == null)
                throw new ArgumentNullException(nameof(rawArguments), "The raw arguments must not be null.");

            if (context == null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (!command.IsEnabled)
                return new CommandDisabledResult(command);

            context.Command = command;
            context.RawArguments = rawArguments;
            try
            {
                var checkResult = await command.RunChecksAsync(context).ConfigureAwait(false);
                if (!checkResult.IsSuccessful)
                    return checkResult;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.Checks, ex);
                await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                return executionFailedResult;
            }

            ArgumentParserResult argumentParserResult;
            try
            {
                IArgumentParser argumentParser;
                if (command.CustomArgumentParserType != null)
                {
                    argumentParser = GetArgumentParser(command.CustomArgumentParserType);
                    if (argumentParser == null)
                        throw new InvalidOperationException($"Custom argument parser of type {command.CustomArgumentParserType} for command {command} not found.");
                }
                else
                {
                    argumentParser = DefaultArgumentParser;
                }

                argumentParserResult = await DefaultArgumentParser.ParseAsync(context).ConfigureAwait(false);
                if (argumentParserResult == null)
                    throw new InvalidOperationException("The result from IArgumentParser.ParseAsync must not be null.");

                if (!argumentParserResult.IsSuccessful)
                    return new ArgumentParseFailedResult(context, argumentParserResult);
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.ArgumentParsing, ex);
                await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                return executionFailedResult;
            }

            object[] parsedArguments;
            try
            {
                var result = await CreateArgumentsAsync(argumentParserResult, context).ConfigureAwait(false);
                if (result.FailedResult != null)
                    return result.FailedResult;

                parsedArguments = result.ParsedArguments;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.TypeParsing, ex);
                await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                return executionFailedResult;
            }

            context.InternalArguments = parsedArguments;
            return await InternalExecuteAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        ///     Attempts to execute the given <see cref="Command"/> with the provided arguments.
        /// </summary>
        /// <param name="command"> The <see cref="Command"/> to execute. </param>
        /// <param name="arguments"> The arguments to use for this <see cref="Command"/>'s parameters. </param>
        /// <param name="context"> The <see cref="CommandContext"/> to use during execution. </param>
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
        public async Task<IResult> ExecuteAsync(Command command, IEnumerable<object> arguments, CommandContext context)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "The command must not be null.");

            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments), "The arguments must not be null.");

            if (context == null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (!command.IsEnabled)
                return new CommandDisabledResult(command);

            context.Command = command;
            context.InternalArguments = arguments.ToArray();
            try
            {
                var checkResult = await command.RunChecksAsync(context).ConfigureAwait(false);
                if (!checkResult.IsSuccessful)
                    return checkResult;
            }
            catch (Exception ex)
            {
                var executionFailedResult = new ExecutionFailedResult(command, CommandExecutionStep.Checks, ex);
                await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                return executionFailedResult;
            }

            return await InternalExecuteAsync(context).ConfigureAwait(false);
        }

        private async Task<IResult> InternalExecuteCallbackAsync(CommandContext context)
        {
            try
            {
                IResult result = null;
                switch (context.Command.Callback)
                {
                    case ModuleBaseCommandCallbackDelegate moduleBaseCallback:
                    {
                        result = await moduleBaseCallback(context).ConfigureAwait(false);
                        if (result is ExecutionFailedResult executionFailedResult)
                        {
                            await InvokeCommandErroredAsync(executionFailedResult, context).ConfigureAwait(false);
                            return result;
                        }
                        break;
                    }

                    case TaskCommandCallbackDelegate callback:
                    {
                        await callback(context).ConfigureAwait(false);
                        break;
                    }

                    case TaskResultCommandCallbackDelegate callback:
                    {
                        result = await callback(context).ConfigureAwait(false);
                        break;
                    }

                    case ValueTaskCommandCallbackDelegate callback:
                    {
                        await callback(context).ConfigureAwait(false);
                        break;
                    }

                    case ValueTaskResultCommandCallbackDelegate callback:
                    {
                        result = await callback(context).ConfigureAwait(false);
                        break;
                    }

                    case VoidCommandCallbackDelegate callback:
                    {
                        callback(context);
                        break;
                    }

                    case ResultCommandCallbackDelegate callback:
                    {
                        result = callback(context);
                        break;
                    }

                    default:
                        throw new InvalidOperationException("Unknown callback type.");
                }

                if (result is CommandResult commandResult)
                    commandResult.Command = context.Command;

                await InvokeCommandExecutedAsync(result as CommandResult, context).ConfigureAwait(false);
                return result ?? new SuccessfulResult();
            }
            catch (Exception ex)
            {
                var result = new ExecutionFailedResult(context.Command, CommandExecutionStep.Command, ex);
                await InvokeCommandErroredAsync(result, context).ConfigureAwait(false);
                return result;
            }
        }

        private async Task<IResult> InternalExecuteAsync(CommandContext context)
        {
            try
            {
                var cooldownResult = context.Command.RunCooldowns(context);
                if (!cooldownResult.IsSuccessful)
                    return cooldownResult;
            }
            catch (Exception ex)
            {
                var result = new ExecutionFailedResult(context.Command, CommandExecutionStep.CooldownBucketKeyGenerating, ex);
                await InvokeCommandErroredAsync(result, context).ConfigureAwait(false);
                return result;
            }

            switch (context.Command.RunMode)
            {
                case RunMode.Sequential:
                    return await InternalExecuteCallbackAsync(context).ConfigureAwait(false);

                case RunMode.Parallel:
                    _ = Task.Run(() => InternalExecuteCallbackAsync(context));
                    return new SuccessfulResult();

                default:
                    throw new InvalidOperationException("Invalid run mode.");
            }
        }

        private async Task<(FailedResult FailedResult, object[] ParsedArguments)> CreateArgumentsAsync(ArgumentParserResult parserResult, CommandContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "The context must not be null.");

            if (context.Command.Parameters.Count == 0)
                return (default, Array.Empty<object>());

            var parsedArguments = new object[context.Command.Parameters.Count];
            for (var i = 0; i < context.Command.Parameters.Count; i++)
            {
                var parameter = context.Command.Parameters[i];
                var hasValue = parserResult.Arguments.TryGetValue(parameter, out var value);
                if (!hasValue && !parameter.IsOptional)
                    throw new InvalidOperationException($"No value for the required parameter {parameter.Name} ({parameter.Type}) was returned by the argument parser ({context.Command.CustomArgumentParserType ?? DefaultArgumentParser.GetType()}).");

                if (!hasValue)
                {
                    parsedArguments[i] = parameter.DefaultValue;
                    continue;
                }

                if (!parameter.IsMultiple)
                {
                    var (result, parsedArgument) = await ParseArgumentAsync(parameter, value, context).ConfigureAwait(false);
                    if (result != null)
                        return (result, default);

                    var checkResult = await parameter.RunChecksAsync(parsedArgument, context).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[i] = parsedArgument;
                }
                else
                {
                    if (!(value is IEnumerable<object> argumentsEnumerable))
                        throw new InvalidOperationException("The multiple parameter requires an enumerable of objects as its argument.");

                    Array array;
                    if (value is IReadOnlyList<object> argumentsList)
                    {
                        array = Array.CreateInstance(parameter.Type, argumentsList.Count);
                        for (var j = 0; j < argumentsList.Count; j++)
                        {
                            var (result, parsedArgument) = await ParseArgumentAsync(parameter, argumentsList[j], context).ConfigureAwait(false);
                            if (result != null)
                                return (result, default);

                            array.SetValue(parsedArgument, j);
                        }
                    }
                    else
                    {
                        array = Array.CreateInstance(parameter.Type, argumentsEnumerable.Count());
                        var j = 0;
                        foreach (var argument in argumentsEnumerable)
                        {
                            var (result, parsedArgument) = await ParseArgumentAsync(parameter, argument, context).ConfigureAwait(false);
                            if (result != null)
                                return (result, default);

                            array.SetValue(parsedArgument, j++);
                        }
                    }

                    var checkResult = await parameter.RunChecksAsync(array, context).ConfigureAwait(false);
                    if (!checkResult.IsSuccessful)
                        return (checkResult as FailedResult, default);

                    parsedArguments[i] = array;
                }
            }

            return (default, parsedArguments);
        }

        private async Task<(TypeParseFailedResult TypeParseFailedResult, object ParsedArgument)> ParseArgumentAsync(Parameter parameter, object argument, CommandContext context)
        {
            if (!(argument is string value))
                return (null, argument);

            IPrimitiveTypeParser primitiveParser;
            if (parameter.CustomTypeParserType != null)
            {
                var customParser = GetSpecificTypeParser(parameter.Type, parameter.CustomTypeParserType);
                if (customParser == null)
                    throw new InvalidOperationException($"Custom type parser of type {parameter.CustomTypeParserType} for parameter {parameter} not found.");

                var typeParserResult = await customParser.ParseAsync(parameter, value, context).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            if (parameter.Type == _stringType)
                return (null, value);

            var parser = GetAnyTypeParser(parameter.Type, (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null);
            if (parser != null)
            {
                var typeParserResult = await parser.ParseAsync(parameter, value, context).ConfigureAwait(false);
                if (!typeParserResult.IsSuccessful)
                    return (new TypeParseFailedResult(parameter, value, typeParserResult.Reason), default);

                return (null, typeParserResult.HasValue ? typeParserResult.Value : null);
            }

            if (primitiveParser == null)
                throw new InvalidOperationException($"No type parser found for parameter {parameter} ({parameter.Type}).");

            if (primitiveParser.TryParse(parameter, value, out var result))
                return (null, result);

            return (new TypeParseFailedResult(parameter, value), default);
        }

        private Task InvokeCommandExecutedAsync(CommandResult result, CommandContext context)
            => _commandExecuted.InvokeAsync(new CommandExecutedEventArgs(result, context));

        private Task InvokeCommandErroredAsync(ExecutionFailedResult result, CommandContext context)
            => _commandExecutionFailed.InvokeAsync(new CommandExecutionFailedEventArgs(result, context));
    }
}
