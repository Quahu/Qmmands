#pragma warning disable IDE0051 // Remove unused private members

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands.Delegates;

namespace Qmmands
{
    internal static class Utilities
    {
        public static bool IsValidModuleDefinition(TypeInfo typeInfo)
            => typeof(IModuleBase).IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters;

        public static bool IsValidTypeParserDefinition(Type parserType, Type parameterType)
        {
            if (!typeof(ITypeParser).IsAssignableFrom(parserType) || parserType.IsAbstract)
                return false;

            var baseType = parserType.BaseType;
            while (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(TypeParser<>))
                baseType = baseType.BaseType;

            return Array.Exists(baseType.GetGenericArguments(), x => x == parameterType);
        }

        public static bool IsValidArgumentParserDefinition(Type parserType)
            => typeof(IArgumentParser).IsAssignableFrom(parserType) && !parserType.IsAbstract;

        public static bool IsNullable(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static Type MakeNullable(Type type)
            => typeof(Nullable<>).MakeGenericType(type);

        public static IEnumerable<TypeInfo> GetValidModules(TypeInfo typeInfo)
        {
            var nestedTypes = typeInfo.GetNestedTypes();
            for (var i = 0; i < nestedTypes.Length; i++)
            {
                var nestedTypeInfo = nestedTypes[i].GetTypeInfo();
                if (IsValidModuleDefinition(nestedTypeInfo))
                    yield return nestedTypeInfo;
            }
        }

        public static IEnumerable<MethodInfo> GetValidCommands(TypeInfo typeInfo)
            => typeInfo.DeclaredMethods.Where(x => x.GetCustomAttribute<CommandAttribute>() != null);

        public static ModuleBuilder CreateModuleBuilder(CommandService service, ModuleBuilder parent, TypeInfo typeInfo)
        {
            if (!IsValidModuleDefinition(typeInfo))
                throw new ArgumentException($"{typeInfo} must not be abstract, must not have generic parameters, and must inherit ModuleBase.", nameof(typeInfo));

            var builder = new ModuleBuilder(typeInfo, parent);
            var attributes = typeInfo.GetCustomAttributes(false);
            for (var i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i])
                {
                    case NameAttribute nameAttribute:
                        builder.WithName(nameAttribute.Value);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Value);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Value);
                        break;

                    case RunModeAttribute runModeAttribute:
                        builder.WithRunMode(runModeAttribute.Value);
                        break;

                    case IgnoresExtraArgumentsAttribute ignoreExtraArgumentsAttribute:
                        builder.WithIgnoresExtraArguments(ignoreExtraArgumentsAttribute.Value);
                        break;

                    case OverrideArgumentParserAttribute overrideArgumentParserAttribute:
                        builder.WithCustomArgumentParserType(overrideArgumentParserAttribute.Value);

                        if (overrideArgumentParserAttribute.GetType() != typeof(OverrideArgumentParserAttribute))
                            builder.AddAttribute(overrideArgumentParserAttribute);
                        break;

                    case GroupAttribute groupAttribute:
                        for (var j = 0; j < groupAttribute.Aliases.Length; j++)
                            builder.AddAlias(groupAttribute.Aliases[j]);
                        break;

                    case CheckAttribute checkAttribute:
                        builder.AddCheck(checkAttribute);
                        break;

                    case DisabledAttribute disabledAttribute:
                        builder.WithIsEnabled(!disabledAttribute.Value);
                        break;

                    case Attribute attribute:
                        builder.AddAttribute(attribute);
                        break;
                }
            }

            foreach (var command in GetValidCommands(typeInfo))
            {
                if (!command.IsPublic || command.IsStatic || command.IsGenericMethod)
                    throw new ArgumentException($"{command} must not be non-public, static, and must not be a generic method.");

                builder.Commands.Add(CreateCommandBuilder(service, builder, typeInfo, command));
            }

            foreach (var submodule in GetValidModules(typeInfo))
                builder.Submodules.Add(CreateModuleBuilder(service, builder, submodule));

            return builder;
        }

        public static CommandBuilder CreateCommandBuilder(CommandService service, ModuleBuilder module, TypeInfo typeInfo, MethodInfo methodInfo)
        {
            var builder = new CommandBuilder(module, CreateModuleBaseCommandCallback(service, typeInfo, methodInfo));
            var attributes = methodInfo.GetCustomAttributes(false);
            for (var i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i])
                {
                    case NameAttribute nameAttribute:
                        builder.WithName(nameAttribute.Value);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Value);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Value);
                        break;

                    case PriorityAttribute priorityAttribute:
                        builder.WithPriority(priorityAttribute.Value);
                        break;

                    case RunModeAttribute runModeAttribute:
                        builder.WithRunMode(runModeAttribute.Value);
                        break;

                    case CooldownAttribute cooldownAttribute:
                        builder.AddCooldown(new Cooldown(cooldownAttribute.Amount, cooldownAttribute.Per, cooldownAttribute.BucketType));
                        break;

                    case IgnoresExtraArgumentsAttribute ignoreExtraArgumentsAttribute:
                        builder.WithIgnoresExtraArguments(ignoreExtraArgumentsAttribute.Value);
                        break;

                    case OverrideArgumentParserAttribute overrideArgumentParserAttribute:
                        builder.WithCustomArgumentParserType(overrideArgumentParserAttribute.Value);

                        if (overrideArgumentParserAttribute.GetType() != typeof(OverrideArgumentParserAttribute))
                            builder.AddAttribute(overrideArgumentParserAttribute);
                        break;

                    case CommandAttribute commandAttribute:
                        for (var j = 0; j < commandAttribute.Aliases.Length; j++)
                            builder.AddAlias(commandAttribute.Aliases[j]);
                        break;

                    case CheckAttribute checkAttribute:
                        builder.AddCheck(checkAttribute);
                        break;

                    case DisabledAttribute disabledAttribute:
                        builder.WithIsEnabled(!disabledAttribute.Value);
                        break;

                    case Attribute attribute:
                        builder.AddAttribute(attribute);
                        break;
                }
            }

            var parameters = methodInfo.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
                builder.Parameters.Add(CreateParameterBuilder(builder, parameters[i], i + 1 == parameters.Length));

            return builder;
        }

        public static ParameterBuilder CreateParameterBuilder(CommandBuilder command, ParameterInfo parameterInfo, bool last)
        {
            var builder = new ParameterBuilder(parameterInfo.ParameterType, command);
            var attributes = parameterInfo.GetCustomAttributes(false);
            for (var i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i])
                {
                    case NameAttribute nameAttribute:
                        builder.WithName(nameAttribute.Value);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Value);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Value);
                        break;

                    case ParamArrayAttribute _:
                        if (!last)
                            throw new ParameterBuildingException(builder, $"A params array parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType}.");

                        builder.WithIsMultiple(true);
                        builder.Type = parameterInfo.ParameterType.GetElementType();
                        break;

                    case RemainderAttribute _:
                        if (!last)
                            throw new ParameterBuildingException(builder, $"A remainder parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType}.");

                        builder.WithIsRemainder(true);
                        break;

                    case OverrideTypeParserAttribute overrideTypeParserAttribute:
                        builder.WithCustomTypeParserType(overrideTypeParserAttribute.Value);

                        if (overrideTypeParserAttribute.GetType() != typeof(OverrideTypeParserAttribute))
                            builder.AddAttribute(overrideTypeParserAttribute);
                        break;

                    case ParameterCheckAttribute parameterCheckAttribute:
                        builder.AddCheck(parameterCheckAttribute);
                        break;

                    case Attribute attribute:
                        builder.AddAttribute(attribute);
                        break;
                }
            }

            if (parameterInfo.HasDefaultValue)
                builder.WithIsOptional(true)
                    .WithDefaultValue(parameterInfo.DefaultValue);

            else if (builder.IsMultiple)
                builder.WithIsOptional(true)
                    .WithDefaultValue(Array.CreateInstance(builder.Type, 0));

            if (builder.Name == null)
                builder.WithName(parameterInfo.Name);

            return builder;
        }

        public static Func<CommandService, IServiceProvider, T> CreateProviderConstructor<T>(CommandService commandService, Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
                throw new InvalidOperationException($"{type} has no public non-static constructors.");

            if (constructors.Length > 1)
                throw new InvalidOperationException($"{type} has multiple public constructors.");

            static object GetDependency(CommandService commandService, ConstructorInfo ctor, IServiceProvider provider, Type serviceType)
            {
                if (serviceType == typeof(IServiceProvider) || serviceType == provider.GetType())
                    return provider;

                if (serviceType == typeof(CommandService) || serviceType == typeof(ICommandService) || serviceType == commandService.GetType())
                    return commandService;

                var service = provider.GetService(serviceType);
                if (service != null)
                    return service;

                throw new InvalidOperationException($"Failed to instantiate {ctor.DeclaringType}, dependency of type {serviceType} was not found.");
            }
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            IList<PropertyInfo> propertiesToInject = new List<PropertyInfo>();
            do
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    if (property.SetMethod != null && !property.SetMethod.IsStatic && property.SetMethod.IsPublic
                        && property.GetCustomAttribute<DoNotInjectAttribute>() == null)
                    {
                        propertiesToInject.Add(property);
                    }
                }

                type = type.BaseType.GetTypeInfo();
            }
            while (type != typeof(object));
            propertiesToInject = (propertiesToInject as List<PropertyInfo>).ToArray();

            return (commandService, provider) =>
            {
                for (var i = 0; i < parameters.Length; i++)
                    arguments[i] = GetDependency(commandService, constructor, provider, parameters[i].ParameterType);

                T instance;
                try
                {
                    instance = (T) constructor.Invoke(arguments);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to instantiate {constructor.DeclaringType}. See the inner exception for more details.", ex);
                }

                for (var i = 0; i < propertiesToInject.Count; i++)
                {
                    var property = propertiesToInject[i];
                    property.SetValue(instance, GetDependency(commandService, constructor, provider, property.PropertyType));
                }

                return instance;
            };
        }

        private static readonly MethodInfo _getGenericTaskResultMethodInfo = typeof(Utilities)
            .GetMethod(nameof(GetGenericTaskResult), BindingFlags.Static | BindingFlags.NonPublic);

        private static async Task<CommandResult> GetGenericTaskResult<T>(Task<T> task) where T : CommandResult
            => task != null ? await task.ConfigureAwait(false) as CommandResult : null;

        private static readonly MethodInfo _getGenericValueTaskResultMethodInfo = typeof(Utilities)
            .GetMethod(nameof(GetGenericValueTaskResult), BindingFlags.Static | BindingFlags.NonPublic);

        private static async ValueTask<CommandResult> GetGenericValueTaskResult<T>(ValueTask<T> task) where T : CommandResult
            => await task.ConfigureAwait(false);

        private delegate T CallbackFunc<T>(object instance, object[] arguments);

        private static CallbackFunc<T> CreateFunc<T>(Expression body, params ParameterExpression[] parameters)
            => Expression.Lambda<CallbackFunc<T>>(body, parameters).Compile();

        private static object CreateDelegate(Type type, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            var instance = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object[]));
            var parameters = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(i)), methodParameters[i].ParameterType);

            var call = Expression.Call(Expression.Convert(instance, type), method, parameters);
            if (method.ReturnType == typeof(void))
            {
                return Expression.Lambda<Action<object, object[]>>(call, instance, arguments).Compile();
            }
            else
            {
                if (!method.ReturnType.IsGenericType)
                {
                    if (typeof(CommandResult).IsAssignableFrom(method.ReturnType))
                    {
                        return CreateFunc<CommandResult>(Expression.Convert(call, typeof(CommandResult)), instance, arguments);
                    }
                    else if (method.ReturnType == typeof(Task))
                    {
                        return CreateFunc<Task>(call, instance, arguments);
                    }
                    else if (method.ReturnType == typeof(ValueTask))
                    {
                        return CreateFunc<ValueTask>(call, instance, arguments);
                    }
                }
                else
                {
                    var genericDefinition = method.ReturnType.GetGenericTypeDefinition();
                    if (genericDefinition == typeof(Task<>))
                    {
                        return CreateFunc<Task<CommandResult>>(
                            Expression.Call(_getGenericTaskResultMethodInfo.MakeGenericMethod(method.ReturnType.GenericTypeArguments[0]), call), instance, arguments);
                    }
                    else if (genericDefinition == typeof(ValueTask<>))
                    {
                        return CreateFunc<ValueTask<CommandResult>>(
                            Expression.Call(_getGenericValueTaskResultMethodInfo.MakeGenericMethod(method.ReturnType.GenericTypeArguments[0]), call), instance, arguments);
                    }
                }
            }

            throw new ArgumentException($"Unsupported method return type: {method.ReturnType}.", nameof(method));
        }

        public static ModuleBaseCommandCallbackDelegate CreateModuleBaseCommandCallback(CommandService service, Type type, MethodInfo method)
        {
            var callbackDelegate = CreateDelegate(type, method);
            var constructor = CreateProviderConstructor<IModuleBase>(service, type);
            return async context =>
            {
                var instance = constructor(service, context.ServiceProvider);
                instance.Prepare(context);

                var executeAfter = true;
                try
                {
                    try
                    {
                        await instance.BeforeExecutedAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        executeAfter = false;
                        return new ExecutionFailedResult(context.Command, CommandExecutionStep.BeforeExecuted, ex);
                    }

                    switch (callbackDelegate)
                    {
                        case CallbackFunc<Task> taskCallback:
                        {
                            await taskCallback(instance, context.InternalArguments).ConfigureAwait(false);
                            break;
                        }

                        case CallbackFunc<Task<CommandResult>> taskResultCallback:
                        {
                            return await taskResultCallback(instance, context.InternalArguments).ConfigureAwait(false);
                        }

                        case CallbackFunc<ValueTask> valueTaskCallback:
                        {
                            await valueTaskCallback(instance, context.InternalArguments).ConfigureAwait(false);
                            break;
                        }

                        case CallbackFunc<ValueTask<CommandResult>> valueTaskResultCallback:
                        {
                            return await valueTaskResultCallback(instance, context.InternalArguments).ConfigureAwait(false);
                        }

                        case Action<object, object[]> voidCallback:
                        {
                            voidCallback(instance, context.InternalArguments);
                            break;
                        }

                        case CallbackFunc<CommandResult> resultCallback:
                        {
                            return resultCallback(instance, context.InternalArguments);
                        }
                    }

                    return null;
                }
                finally
                {
                    try
                    {
                        if (executeAfter)
                            await instance.AfterExecutedAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        if (instance is IDisposable disposable)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch { }
                        }

#if NETCOREAPP3_0
                        if (instance is IAsyncDisposable asyncDisposable)
                        {
                            try
                            {
                                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                            }
                            catch { }
                        }
#endif
                    }
                }
            };
        }

        public static readonly IReadOnlyDictionary<Type, Delegate> TryParseDelegates;

        public static IPrimitiveTypeParser CreatePrimitiveTypeParser(Type type)
            => Activator.CreateInstance(typeof(PrimitiveTypeParser<>).MakeGenericType(type)) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateEnumTypeParser(Type type, Type enumType, CommandService service)
            => Activator.CreateInstance(typeof(EnumTypeParser<>).MakeGenericType(type), new object[] { enumType, service }) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateNullableEnumTypeParser(Type type, IPrimitiveTypeParser enumTypeParser)
            => Activator.CreateInstance(typeof(NullableEnumTypeParser<>).MakeGenericType(type), new[] { enumTypeParser }) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateNullablePrimitiveTypeParser(Type type, IPrimitiveTypeParser primitiveTypeParser)
            => Activator.CreateInstance(typeof(NullablePrimitiveTypeParser<>).MakeGenericType(type), new[] { primitiveTypeParser }) as IPrimitiveTypeParser;

        public static ITypeParser CreateNullableTypeParser(Type nullableType, ITypeParser typeParser)
            => Activator.CreateInstance(typeof(NullableTypeParser<>).MakeGenericType(nullableType), new object[] { typeParser }) as ITypeParser;

        static Utilities()
        {
            TryParseDelegates = new Dictionary<Type, Delegate>(13)
            {
                [typeof(char)] = (TryParseDelegate<char>) TryParseChar,
                [typeof(bool)] = (TryParseDelegate<bool>) bool.TryParse,
                [typeof(byte)] = (TryParseDelegate<byte>) byte.TryParse,
                [typeof(sbyte)] = (TryParseDelegate<sbyte>) sbyte.TryParse,
                [typeof(short)] = (TryParseDelegate<short>) short.TryParse,
                [typeof(ushort)] = (TryParseDelegate<ushort>) ushort.TryParse,
                [typeof(int)] = (TryParseDelegate<int>) int.TryParse,
                [typeof(uint)] = (TryParseDelegate<uint>) uint.TryParse,
                [typeof(long)] = (TryParseDelegate<long>) long.TryParse,
                [typeof(ulong)] = (TryParseDelegate<ulong>) ulong.TryParse,
                [typeof(float)] = (TryParseDelegate<float>) float.TryParse,
                [typeof(double)] = (TryParseDelegate<double>) double.TryParse,
                [typeof(decimal)] = (TryParseDelegate<decimal>) decimal.TryParse
            };
        }

        private static bool TryParseChar(ReadOnlySpan<char> value, out char result)
        {
            if (value.Length == 1)
            {
                result = value[0];
                return true;
            }

            result = default;
            return false;
        }

        public static bool IsNumericType(Type type)
            => type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);

        public static bool IsStringType(Type type)
            => type == typeof(string);

        public static bool IsNumericOrStringType(Type type)
            => IsNumericType(type) || IsStringType(type);

        public static bool IsCaseSensitive(this StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                case StringComparison.Ordinal:
                    return true;

                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison));
            }
        }

        public static ImmutableArray<T> TryMoveToImmutable<T>(this ImmutableArray<T>.Builder builder)
            => builder.Capacity == builder.Count
                ? builder.MoveToImmutable()
                : builder.ToImmutable();

        internal sealed class CommandOverloadComparer : IComparer<CommandMatch>
        {
            public static readonly CommandOverloadComparer Instance = new CommandOverloadComparer();

            private CommandOverloadComparer()
            { }

            public int Compare(CommandMatch x, CommandMatch y)
            {
                var pathCompare = y.Path.Count.CompareTo(x.Path.Count);
                if (pathCompare != 0)
                    return pathCompare;

                var priorityCompare = y.Command.Priority.CompareTo(x.Command.Priority);
                if (priorityCompare != 0)
                    return priorityCompare;

                var parametersCompare = y.Command.Parameters.Count.CompareTo(x.Command.Parameters.Count);
                return parametersCompare;
            }
        }
    }
}
