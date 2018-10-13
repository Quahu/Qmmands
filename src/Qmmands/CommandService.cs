using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        ///     Gets whether <see cref="FindCommands"/> is case sensitive or not.
        /// </summary>
        public bool CaseSensitive { get; }

        /// <summary>
        ///     Gets the default <see cref="RunMode"/> for commands.
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
        ///     Gets the parameter parser.
        /// </summary>
        public IArgumentParser ParameterParser { get; }

        internal StringComparison StringComparison { get; }

        /// <summary>
        ///     Fires when a command is successfully executed. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        public event Func<Command, CommandResult, ICommandContext, IServiceProvider, Task> CommandExecuted
        {
            add => _commandExecuted.Hook(value);
            remove => _commandExecuted.Unhook(value);
        }
        private readonly AsyncEvent<Func<Command, CommandResult, ICommandContext, IServiceProvider, Task>> _commandExecuted = new AsyncEvent<Func<Command, CommandResult, ICommandContext, IServiceProvider, Task>>();

        /// <summary>
        ///     Fires when a command fails to execute. Use this to handle <see cref="RunMode.Parallel"/> commands.
        /// </summary>
        public event Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task> CommandErrored
        {
            add => _commandErrored.Hook(value);
            remove => _commandErrored.Unhook(value);
        }
        private readonly AsyncEvent<Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task>> _commandErrored = new AsyncEvent<Func<ExecutionFailedResult, ICommandContext, IServiceProvider, Task>>();

        /// <summary>
        ///     Fires when a non-user instantiated <see cref="ModuleBuilder"/> is about to be built into a <see cref="Module"/>.
        /// </summary>
        public event Func<ModuleBuilder, Task> ModuleBuilding
        {
            add => _moduleBuilding.Hook(value);
            remove => _moduleBuilding.Unhook(value);
        }
        private readonly AsyncEvent<Func<ModuleBuilder, Task>> _moduleBuilding = new AsyncEvent<Func<ModuleBuilder, Task>>();

        private readonly ConcurrentDictionary<Type, Dictionary<Type, (bool, ITypeParser)>> _parsers;
        private readonly ConcurrentDictionary<Type, IPrimitiveTypeParser> _primitiveParsers;
        private readonly Dictionary<Type, Module> _typeModules;
        private readonly HashSet<Module> _modules;
        private readonly CommandMap _map;
        private static readonly Type _stringType = typeof(string);
        private readonly object _moduleLock = new object();
        private readonly SemaphoreSlim _moduleSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the specified <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        /// <param name="configuration"> The <see cref="CommandServiceConfiguration"/> to use. </param>
        /// <exception cref="ArgumentNullException"> Command service's configuration mustn't be null. </exception>
        public CommandService(CommandServiceConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("Command service's configuration mustn't be null.", nameof(configuration));

            CaseSensitive = configuration.CaseSensitive;
            DefaultRunMode = configuration.DefaultRunMode;
            IgnoreExtraArguments = configuration.IgnoreExtraArguments;
            Separator = configuration.Separator;
            SeparatorRequirement = configuration.SeparatorRequirement;
            ParameterParser = configuration.ArgumentParser;
            StringComparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            _typeModules = new Dictionary<Type, Module>();
            _modules = new HashSet<Module>();
            _map = new CommandMap(this);
            _parsers = new ConcurrentDictionary<Type, Dictionary<Type, (bool, ITypeParser)>>();
            _primitiveParsers = new ConcurrentDictionary<Type, IPrimitiveTypeParser>();
            foreach (var type in TypeParserUtils.TryParseDelegates.Keys)
            {
                var primitiveTypeParser = TypeParserUtils.CreatePrimitiveTypeParser(type);
                _primitiveParsers.TryAdd(type, primitiveTypeParser);
                _primitiveParsers.TryAdd(typeof(Nullable<>).MakeGenericType(type), TypeParserUtils.CreateNullablePrimitiveTypeParser(type, primitiveTypeParser));
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
        /// <returns> An enumerable with all <see cref="Command"/>s. </returns>
        public IEnumerable<Command> GetCommands()
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
            {
                foreach (var module in _modules)
                    foreach (var command in GetCommands(module))
                        yield return command;
            }
        }

        /// <summary>
        ///     Enumerates through all of the added <see cref="Module"/>s yields them.
        /// </summary>
        /// <returns> An enumerable with all <see cref="Module"/>s. </returns>
        public IEnumerable<Module> GetModules()
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
                foreach (var module in _modules)
                {
                    yield return module;

                    foreach (var submodule in GetSubmodules(module))
                        yield return submodule;
                }
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the provided path.
        /// </summary>
        /// <param name="path"> The path to use for searching. </param>
        /// <returns> An ordered enumerable of <see cref="CommandMatch"/>es. </returns>
        public IEnumerable<CommandMatch> FindCommands(string path)
            => _map.FindCommands(path).OrderByDescending(x => x.Path.Length)
                .ThenByDescending(x => x.Command.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length) // a bad solution to bad people using whitespace in command names
                .ThenByDescending(x => x.Command.Parameters.Count);

        /// <summary>
        ///     Adds a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T"> The type to add the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to add for the type. </param>
        /// <param name="replacePrimitive"> Whether to replace the primitive parser. </param>
        public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false)
            => AddParserInternal(typeof(T), parser, replacePrimitive);

        private void AddParserInternal(Type type, ITypeParser parser, bool replacePrimitive = false)
        {
            if (type.IsEnum)
                throw new ArgumentException("Custom enum type parsers aren't supported.", nameof(type));

            _parsers.AddOrUpdate(type,
            new Dictionary<Type, (bool, ITypeParser)> { [parser.GetType()] = (replacePrimitive, parser) },
            (k, v) =>
            {
                v.Add(k, (replacePrimitive, parser));
                return v;
            });
            if (type.IsValueType)
            {
                var nullableParser = TypeParserUtils.CreateNullableTypeParser(type, parser);
                _parsers.AddOrUpdate(typeof(Nullable<>).MakeGenericType(type),
                    new Dictionary<Type, (bool, ITypeParser)> { [nullableParser.GetType()] = (replacePrimitive, nullableParser) },
                    (k, v) =>
                    {
                        v.Add(k, (replacePrimitive, nullableParser));
                        return v;
                    });
            }
        }

        /// <summary>
        ///     Removes a <see cref="TypeParser{T}"/> for the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> The type to remove the <paramref name="parser"/> for. </typeparam>
        /// <param name="parser"> The <see cref="TypeParser{T}"/> to remove for the type. </param>
        public void RemoveTypeParser<T>(TypeParser<T> parser)
        {
            var type = typeof(T);
            if (!_parsers.ContainsKey(type))
                throw new ArgumentException($"A parser for type {type.Name} hasn't been added.", nameof(T));

            RemoveParserInternal(type, parser);
        }

        private void RemoveParserInternal(Type type, ITypeParser parser)
        {
            if (_parsers.TryGetValue(type, out var typeParsers))
            {
                typeParsers.Remove(parser.GetType());

                if (type.IsValueType)
                    typeParsers.Remove(typeof(Nullable<>).MakeGenericType(type));
            }
        }

        internal ITypeParser GetSpecificTypeParser(Type type, Type parserType)
        {
            if (_parsers.TryGetValue(type, out var typeParsers))
            {
                if (typeParsers.TryGetValue(parserType, out var typeParser))
                    return typeParser.Item2;
            }

            return null;
        }

        internal ITypeParser GetAnyTypeParser(Type type, bool replacing)
        {
            if (_parsers.TryGetValue(type, out var typeParsers))
            {
                if (replacing)
                {
                    var filtered = typeParsers.Where(x => x.Value.Item1).ToArray();
                    if (filtered.Length > 0)
                        return filtered[0].Value.Item2;
                }

                else
                    return typeParsers.First().Value.Item2;
            }

            return null;
        }

        internal IPrimitiveTypeParser GetPrimitiveTypeParser(Type type)
        {
            if (_primitiveParsers.TryGetValue(type, out var typeParser))
                return typeParser;

            if (type.IsEnum)
            {
                var enumParser = TypeParserUtils.CreateEnumTypeParser(type.GetEnumUnderlyingType(), type, !CaseSensitive);
                _primitiveParsers.TryAdd(type, enumParser);
                _primitiveParsers.TryAdd(typeof(Nullable<>).MakeGenericType(type), TypeParserUtils.CreateNullableEnumTypeParser(type.GetEnumUnderlyingType(), enumParser));
                return GetPrimitiveTypeParser(type);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && (type = type.GetGenericArguments()[0]).IsEnum)
                return GetPrimitiveTypeParser(type);

            return null;
        }

        /// <summary>
        ///     Attempts to add all valid <see cref="Module"/>s and <see cref="Command"/>s found in the provided <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly"> The assembly to search. </param>
        /// <returns> A list of all found and added modules. </returns>
        public async Task<IReadOnlyList<Module>> AddModulesAsync(Assembly assembly)
        {
            var modules = new List<Module>();
            var types = assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i].GetTypeInfo();
                if (!ReflectionUtils.IsValidModuleDefinition(typeInfo) || typeInfo.IsNested || typeInfo.GetCustomAttribute<DontAutoAddAttribute>() != null)
                    continue;

                var methods = typeInfo.GetMethods();
                for (var j = 0; j < methods.Length; j++)
                {
                    if (ReflectionUtils.IsValidCommandDefinition(methods[j]))
                    {
                        modules.Add(await AddModuleAsync(typeInfo.AsType()).ConfigureAwait(false));
                        break;
                    }
                }
            }

            return modules.AsReadOnly();
        }

        /// <summary>
        ///     Attempts to build the specified <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builder"> The builder to build. </param>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        public async Task<Module> AddModuleAsync(ModuleBuilder builder)
        {
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
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        public Task<Module> AddModuleAsync(Action<ModuleBuilder> builderAction)
        {
            var builder = new ModuleBuilder();
            builderAction(builder);
            return AddModuleAsync(builder);
        }

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> type as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The type to add. </typeparam>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        public Task<Module> AddModuleAsync<TModule>()
            => AddModuleAsync(typeof(TModule));

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        public async Task<Module> AddModuleAsync(Type type)
        {
            if (_typeModules.ContainsKey(type))
                throw new ArgumentException($"{type.Name} has already been added as a module.", nameof(type));
            try
            {

                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                var moduleBuilder = ReflectionUtils.BuildModule(type.GetTypeInfo());
                await _moduleBuilding.InvokeAsync(moduleBuilder).ConfigureAwait(false);
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

            _map.AddModule(module, new Stack<string>());
            _modules.Add(module);
            AddSubmodules(module);
        }

        /// <summary>
        ///     Removes the specified <see cref="Module"/>.
        /// </summary>
        /// <param name="module"></param>
        /// <exception cref="ArgumentException"> The module isn't held by this instance. </exception>
        public async Task RemoveModuleAsync(Module module)
        {
            void RemoveSubmodules(Module m)
            {
                foreach (var submodule in m.Submodules)
                {
                    _typeModules.Remove(submodule.Type);
                    RemoveSubmodules(submodule);
                }
            }

            if (!_modules.Contains(module))
                throw new ArgumentException("This module hasn't been added.", nameof(module));

            try
            {
                await _moduleSemaphore.WaitAsync().ConfigureAwait(false);
                _map.RemoveModule(module, new Stack<string>());
                _modules.Remove(module);
                if (module.Type != null)
                {
                    _typeModules.Remove(module.Type);
                    RemoveSubmodules(module);
                }
            }
            finally
            {
                _moduleSemaphore.Release();
            }
        }

        /// <summary>
        ///     Attempts to find <see cref="Command"/>s matching the input and executes the most suitable one.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns></returns>
        public async Task<IResult> ExecuteAsync(string input, ICommandContext context, IServiceProvider provider = null)
        {
            if (provider is null)
                provider = EmptyServiceProvider.Instance;

            var matches = FindCommands(input).ToArray();
            if (matches.Length == 0)
                return new CommandNotFoundResult();

            var grouppedMatches = matches.GroupBy(x => string.Join(Separator, x.Path));
            foreach (var group in grouppedMatches)
            {
                var failedOverloads = new Dictionary<Command, FailedResult>();
                var overloadCount = 0;
                foreach (var match in group.OrderByDescending(x => x.Command.Priority))
                {
                    overloadCount++;
                    try
                    {
                        var checkRunResult = await match.Command.RunChecksAsync(context, provider).ConfigureAwait(false);
                        if (!checkRunResult.IsSuccessful)
                            return checkRunResult;
                    }
                    catch (Exception ex)
                    {
                        return new ExecutionFailedResult(match.Command, CommandExecutionStep.Checks, ex);
                    }

                    ParseResult parseResult;
                    try
                    {
                        parseResult = ParameterParser.ParseRawArguments(match.Command, match.RawArguments);
                        if (!parseResult.IsSuccessful)
                        {
                            failedOverloads.Add(match.Command, new ParseFailedResult(match.Command, parseResult));
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        return new ExecutionFailedResult(match.Command, CommandExecutionStep.ArgumentParsing, ex);
                    }

                    var parsedArguments = new List<object>();
                    var skipOverload = false;
                    foreach (var kvp in parseResult.Arguments)
                    {
                        var parameter = kvp.Key;
                        async Task<bool> ParseArgumentAsync(IList list, object argument)
                        {
                            if (!(argument is string value))
                            {
                                if (list == null)
                                    parsedArguments.Add(kvp.Value);

                                else
                                    list.Add(kvp.Value);

                                return true;
                            }

                            if (parameter.Type == _stringType)
                            {
                                if (list == null)
                                    parsedArguments.Add(value);

                                else
                                    list.Add(value);

                                return true;
                            }

                            IPrimitiveTypeParser primitiveParser;
                            try
                            {
                                if (!(parameter.CustomTypeParserType is null))
                                {
                                    var customParser = GetSpecificTypeParser(parameter.Type, parameter.CustomTypeParserType);
                                    if (customParser is null)
                                        throw new InvalidOperationException($"Custom parser of type {parameter.CustomTypeParserType.Name} for parameter {parameter.Name} not found.");

                                    var typeParserResult = await customParser.ParseAsync(value, context, provider).ConfigureAwait(false);
                                    if (!typeParserResult.IsSuccessful)
                                    {
                                        failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, typeParserResult.Error));
                                        return false;
                                    }

                                    if (list == null)
                                        parsedArguments.Add(typeParserResult.Value);

                                    else
                                        list.Add(typeParserResult.Value);

                                    return true;
                                }

                                var parser = GetAnyTypeParser(parameter.Type, (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null);
                                if (!(parser is null))
                                {
                                    var typeParserResult = await parser.ParseAsync(value, context, provider).ConfigureAwait(false);
                                    if (!typeParserResult.IsSuccessful)
                                    {
                                        failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, typeParserResult.Error));
                                        return false;
                                    }

                                    if (list == null)
                                        parsedArguments.Add(typeParserResult.Value);

                                    else
                                        list.Add(typeParserResult.Value);

                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                failedOverloads.Add(match.Command, new ExecutionFailedResult(match.Command, CommandExecutionStep.TypeParsing, ex));
                                return false;
                            }

                            if (primitiveParser != null || (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null)
                            {
                                if (!primitiveParser.TryParse(value, out var result))
                                {
                                    failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, $"Failed to parse {parameter.Type}."));
                                    return false;
                                }

                                if (list == null)
                                    parsedArguments.Add(result);

                                else
                                    list.Add(result);

                                return true;
                            }

                            failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, $"No type parser found for parameter {parameter} ({parameter.Type})."));
                            return false;
                        }

                        if (kvp.Value is IEnumerable<string> multipleArguments)
                        {
                            var list = new List<object>();
                            foreach (var argument in multipleArguments)
                            {
                                if (!await ParseArgumentAsync(list, argument).ConfigureAwait(false))
                                {
                                    skipOverload = true;
                                    break;
                                }

                            }
                            var array = Array.CreateInstance(parameter.Type, list.Count);
                            for (var i = 0; i < list.Count; i++)
                                array.SetValue(list[i], i);
                            parsedArguments.Add(array);
                        }

                        else if (!await ParseArgumentAsync(null, kvp.Value).ConfigureAwait(false))
                        {
                            skipOverload = true;
                            break;
                        }
                    }

                    if (skipOverload)
                        continue;

                    switch (match.Command.RunMode)
                    {

                        case RunMode.Sequential:
                            return await ExecuteInternalAsync(match.Command, context, provider, parsedArguments.ToArray()).ConfigureAwait(false);

                        case RunMode.Parallel:
                            _ = Task.Run(() => ExecuteInternalAsync(match.Command, context, provider, parsedArguments.ToArray()));
                            return new SuccessfulResult();
                    }
                }

                if (failedOverloads.Count <= 0)
                    continue;

                return overloadCount == 1 ? failedOverloads.First().Value : new OverloadNotFoundResult(failedOverloads);
            }

            throw new Exception("Shouldn't happen? :^)");
        }

        private async Task<IResult> ExecuteInternalAsync(Command command, ICommandContext context, IServiceProvider provider, object[] arguments)
        {
            try
            {
                var result = await command.Callback(command, context, provider, arguments);
                await _commandExecuted.InvokeAsync(command, result as CommandResult, context, provider).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                var result = new ExecutionFailedResult(command, CommandExecutionStep.Command, ex);
                await _commandErrored.InvokeAsync(result, context, provider).ConfigureAwait(false);
                return result;
            }
        }
    }
}
