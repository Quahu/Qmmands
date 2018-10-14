using System;
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
        private readonly SemaphoreSlim _moduleSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        ///     Initialises a new <see cref="CommandService"/> with the specified <see cref="CommandServiceConfiguration"/>.
        /// </summary>
        /// <param name="configuration"> The <see cref="CommandServiceConfiguration"/> to use. </param>
        /// <exception cref="ArgumentNullException"> Command service's configuration mustn't be null. </exception>
        public CommandService(CommandServiceConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Command service's configuration mustn't be null.");

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

            foreach (var module in _modules)
                foreach (var command in GetCommands(module))
                    yield return command;
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

            foreach (var module in _modules)
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
        /// <exception cref="ArgumentNullException"> The type parser to add mustn't be null. </exception>
        /// <exception cref="ArgumentException"> Custom enum type parsers aren't supported. </exception>
        public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to add mustn't be null.");

            var type = typeof(T);
            if (type.IsEnum)
                throw new ArgumentException("Custom enum type parsers aren't supported.", nameof(type));

            AddParserInternal(typeof(T), parser, replacePrimitive);
        }

        private void AddParserInternal(Type type, ITypeParser parser, bool replacePrimitive = false)
        {
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
        /// <exception cref="ArgumentNullException"> The type parser to remove mustn't be null. </exception>
        /// <exception cref="ArgumentException"> A parser for this type hasn't been added. </exception>
        public void RemoveTypeParser<T>(TypeParser<T> parser)
        {
            if (parser is null)
                throw new ArgumentNullException(nameof(parser), "The type parser to remove mustn't be null.");

            var type = typeof(T);
            if (!_parsers.ContainsKey(type))
                throw new ArgumentException($"A parser for type {type.Name} hasn't been added.", nameof(T));

            RemoveParserInternal(type, parser);
        }

        private void RemoveParserInternal(Type type, ITypeParser parser)
        {
            if (!_parsers.TryGetValue(type, out var typeParsers))
                return;

            typeParsers.Remove(parser.GetType());

            if (type.IsValueType)
                typeParsers.Remove(typeof(Nullable<>).MakeGenericType(type));
        }

        internal ITypeParser GetSpecificTypeParser(Type type, Type parserType)
            => _parsers.TryGetValue(type, out var typeParsers) && typeParsers.TryGetValue(parserType, out var typeParser) ? typeParser.Item2 : null;

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
        /// <exception cref="ArgumentNullException"> The assembly to add modules from mustn't be null. </exception>
        public async Task<IReadOnlyList<Module>> AddModulesAsync(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly), "The assembly to add modules from mustn't be null.");

            var modules = new List<Module>();
            var types = assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i].GetTypeInfo();
                if (!ReflectionUtils.IsValidModuleDefinition(typeInfo) || typeInfo.IsNested || typeInfo.GetCustomAttribute<DontAutoAddAttribute>() != null)
                    continue;

                modules.Add(await AddModuleAsync(typeInfo.AsType()).ConfigureAwait(false));
            }

            return modules.AsReadOnly();
        }

        /// <summary>
        ///     Attempts to build the specified <see cref="ModuleBuilder"/> into a <see cref="Module"/>.
        /// </summary>
        /// <param name="builder"> The builder to build. </param>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        /// <exception cref="ArgumentNullException"> The module builder to add mustn't be null. </exception>
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
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        /// <exception cref="ArgumentNullException"> The module builder action mustn't be null. </exception>
        public Task<Module> AddModuleAsync(Action<ModuleBuilder> builderAction)
        {
            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction), "The module builder action mustn't be null.");

            var builder = new ModuleBuilder();
            builderAction(builder);
            return AddModuleAsync(builder);
        }

        /// <summary>
        ///     Attempts to add the specified <typeparamref name="TModule"/> type as a <see cref="Module"/>. 
        /// </summary>
        /// <typeparam name="TModule"> The type to add. </typeparam>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        /// <exception cref="ArgumentException"> The type has already been added as a module. </exception>
        public Task<Module> AddModuleAsync<TModule>()
            => AddModuleAsync(typeof(TModule));

        /// <summary>
        ///     Attempts to add the specified <see cref="Type"/> as a <see cref="Module"/>. 
        /// </summary>
        /// <returns> A <see cref="Module"/> if succeeded. </returns>
        /// <exception cref="ArgumentNullException"> The type to add mustn't be null. </exception>
        /// <exception cref="ArgumentException"> The type has already been added as a module. </exception>
        public async Task<Module> AddModuleAsync(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type), "The type to add mustn't be null.");

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
        /// <exception cref="ArgumentNullException"> The module to remove mustn't be null. </exception>
        /// <exception cref="ArgumentException"> The module isn't held by this instance of <see cref="CommandService"/>. </exception>
        public async Task RemoveModuleAsync(Module module)
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module), "The module to remove mustn't be null.");

            if (!_modules.Contains(module))
                throw new ArgumentException("This module hasn't been added.", nameof(module));

            void RemoveSubmodules(Module m)
            {
                foreach (var submodule in m.Submodules)
                {
                    _typeModules.Remove(submodule.Type);
                    RemoveSubmodules(submodule);
                }
            }

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
        /// <returns> An <see cref="IResult"/>. </returns>
        /// <exception cref="ArgumentNullException"> The input mustn't be null. </exception>
        public async Task<IResult> ExecuteAsync(string input, ICommandContext context, IServiceProvider provider = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "The input mustn't be null.");

            if (provider is null)
                provider = EmptyServiceProvider.Instance;

            var matches = FindCommands(input).ToArray();
            if (matches.Length == 0)
                return new CommandNotFoundResult();

            var groupedMatches = matches.GroupBy(x => string.Join(Separator, x.Path));
            foreach (var group in groupedMatches)
            {
                var failedOverloads = new Dictionary<Command, FailedResult>();
                var overloadCount = 0;
                foreach (var match in group.OrderByDescending(x => x.Command.Priority))
                {
                    overloadCount++;
                    try
                    {
                        var checkResult = await match.Command.RunChecksAsync(context, provider).ConfigureAwait(false);
                        if (!checkResult.IsSuccessful)
                            return checkResult;
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
                        async Task<(bool Success, object Parsed)> ParseArgumentAsync(object argument)
                        {
                            if (!(argument is string value))
                                return (true, kvp.Value);

                            if (parameter.Type == _stringType)
                                return (true, value);

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
                                        return (false, default);
                                    }

                                    return (true, typeParserResult.Value);
                                }

                                var parser = GetAnyTypeParser(parameter.Type, (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null);
                                if (!(parser is null))
                                {
                                    var typeParserResult = await parser.ParseAsync(value, context, provider).ConfigureAwait(false);
                                    if (!typeParserResult.IsSuccessful)
                                    {
                                        failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, typeParserResult.Error));
                                        return (false, default);
                                    }

                                    return (true, typeParserResult.Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                failedOverloads.Add(match.Command, new ExecutionFailedResult(match.Command, CommandExecutionStep.TypeParsing, ex));
                                return (false, default);
                            }

                            if (primitiveParser != null || (primitiveParser = GetPrimitiveTypeParser(parameter.Type)) != null)
                            {
                                if (!primitiveParser.TryParse(value, out var result))
                                {
                                    failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, $"Failed to parse {parameter.Type}."));
                                    return (false, default);
                                }

                                return (true, result);
                            }

                            failedOverloads.Add(match.Command, new TypeParserFailedResult(parameter, value, $"No type parser found for parameter {parameter} ({parameter.Type})."));
                            return (false, false);
                        }

                        if (kvp.Value is IEnumerable<string> multipleArguments)
                        {
                            var list = new List<object>();
                            foreach (var argument in multipleArguments)
                            {
                                var (success, parsed) = await ParseArgumentAsync(argument).ConfigureAwait(false);
                                if (!success)
                                {
                                    skipOverload = true;
                                    break;
                                }

                                list.Add(parsed);
                            }

                            var array = Array.CreateInstance(parameter.Type, list.Count);
                            for (var i = 0; i < list.Count; i++)
                                array.SetValue(list[i], i);

                            var checkResult = await parameter.RunChecksAsync(array, context, provider);
                            if (!checkResult.IsSuccessful)
                            {
                                failedOverloads.Add(match.Command, checkResult as FailedResult);
                                skipOverload = true;
                                break;
                            }

                            parsedArguments.Add(array);
                        }

                        else
                        {
                            var (success, parsed) = await ParseArgumentAsync(kvp.Value).ConfigureAwait(false);
                            if (!success)
                            {
                                skipOverload = true;
                                break;
                            }

                            var checkResult = await parameter.RunChecksAsync(parsed, context, provider);
                            if (!checkResult.IsSuccessful)
                            {
                                failedOverloads.Add(match.Command, checkResult as FailedResult);
                                skipOverload = true;
                                break;
                            }

                            parsedArguments.Add(parsed);
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
