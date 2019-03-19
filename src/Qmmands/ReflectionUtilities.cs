#pragma warning disable IDE0051 // Remove unused private members

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Qmmands
{
    internal static class ReflectionUtilities
    {
        public static bool IsValidModuleDefinition(TypeInfo typeInfo)
            => typeof(IModuleBase).IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters;

        public static bool IsValidCommandDefinition(MethodInfo methodInfo)
        {
            if (!methodInfo.IsPublic || methodInfo.IsStatic || methodInfo.GetCustomAttribute<CommandAttribute>() == null)
                return false;

            if (methodInfo.ReturnType == typeof(Task))
                return true;

            if (methodInfo.ReturnType.GenericTypeArguments.Length == 0)
                return false;

            var genericTypeDefinition = methodInfo.ReturnType.GetGenericTypeDefinition();
            return (genericTypeDefinition == typeof(Task<>)
#if NETCOREAPP
                || genericTypeDefinition == typeof(ValueTask<>)
#endif
                )
                && typeof(CommandResult).IsAssignableFrom(methodInfo.ReturnType.GenericTypeArguments[0]);
        }

        public static bool IsValidParserDefinition(Type parserType, Type parameterType)
            => typeof(ITypeParser).IsAssignableFrom(parserType) && !parserType.IsAbstract && Array.Exists(parserType.BaseType.GetGenericArguments(), x => x == parameterType);

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
            => typeInfo.DeclaredMethods.Where(IsValidCommandDefinition);

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
                        builder.WithName(nameAttribute.Name);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Description);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Remarks);
                        break;

                    case RunModeAttribute runModeAttribute:
                        builder.WithRunMode(runModeAttribute.RunMode);
                        break;

                    case IgnoreExtraArgumentsAttribute ignoreExtraArgumentsAttribute:
                        builder.WithIgnoresExtraArguments(ignoreExtraArgumentsAttribute.IgnoreExtraArguments);
                        break;

                    case GroupAttribute groupAttribute:
                        builder.AddAliases(groupAttribute.Aliases);
                        break;

                    case CheckAttribute checkAttribute:
                        builder.AddCheck(checkAttribute);
                        break;

                    case Attribute attribute:
                        builder.AddAttribute(attribute);
                        break;
                }
            }

            foreach (var command in GetValidCommands(typeInfo))
                builder.Commands.Add(CreateCommandBuilder(service, builder, typeInfo, command));

            foreach (var submodule in GetValidModules(typeInfo))
                builder.Submodules.Add(CreateModuleBuilder(service, builder, submodule));

            return builder;
        }

        public static CommandBuilder CreateCommandBuilder(CommandService service, ModuleBuilder module, TypeInfo typeInfo, MethodInfo methodInfo)
        {
            var builder = new CommandBuilder(module, CreateCommandCallback(service, typeInfo, methodInfo));
            var attributes = methodInfo.GetCustomAttributes(false);
            for (var i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i])
                {
                    case NameAttribute nameAttribute:
                        builder.WithName(nameAttribute.Name);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Description);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Remarks);
                        break;

                    case PriorityAttribute priorityAttribute:
                        builder.WithPriority(priorityAttribute.Priority);
                        break;

                    case RunModeAttribute runModeAttribute:
                        builder.WithRunMode(runModeAttribute.RunMode);
                        break;

                    case CooldownAttribute cooldownAttribute:
                        builder.AddCooldown(new Cooldown(cooldownAttribute.Amount, cooldownAttribute.Per, cooldownAttribute.BucketType));
                        break;

                    case IgnoreExtraArgumentsAttribute ignoreExtraArgumentsAttribute:
                        builder.WithIgnoresExtraArguments(ignoreExtraArgumentsAttribute.IgnoreExtraArguments);
                        break;

                    case CommandAttribute commandAttribute:
                        builder.AddAliases(commandAttribute.Aliases);
                        break;

                    case CheckAttribute checkAttribute:
                        builder.AddCheck(checkAttribute);
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
            var builder = new ParameterBuilder(command);
            var attributes = parameterInfo.GetCustomAttributes(false);
            for (var i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i])
                {
                    case NameAttribute nameAttribute:
                        builder.WithName(nameAttribute.Name);
                        break;

                    case DescriptionAttribute descriptionAttribute:
                        builder.WithDescription(descriptionAttribute.Description);
                        break;

                    case RemarksAttribute remarksAttribute:
                        builder.WithRemarks(remarksAttribute.Remarks);
                        break;

                    case ParamArrayAttribute _:
                        if (!last)
                            throw new ParameterBuildingException(builder, $"A params array parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType}.");

                        builder.WithIsMultiple(true)
                            .WithType(parameterInfo.ParameterType.GetElementType());
                        break;

                    case RemainderAttribute _:
                        if (!last)
                            throw new ParameterBuildingException(builder, $"A remainder parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType}.");

                        builder.WithIsRemainder(true);
                        break;

                    case OverrideTypeParserAttribute overwriteTypeParserAttribute:
                        builder.WithCustomTypeParserType(overwriteTypeParserAttribute.CustomTypeParserType);

                        if (overwriteTypeParserAttribute.GetType() != typeof(OverrideTypeParserAttribute))
                            builder.AddAttribute(overwriteTypeParserAttribute);
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

            if (!builder.IsMultiple)
                builder.WithType(parameterInfo.ParameterType);

            if (builder.Name is null)
                builder.WithName(parameterInfo.Name);

            return builder;
        }

        public static Func<IServiceProvider, T> CreateProviderConstructor<T>(CommandService commandService, Type type)
        {
            object GetDependency(IServiceProvider provider, Type serviceType)
            {
                if (serviceType == typeof(IServiceProvider) || serviceType == provider.GetType())
                    return provider;

                if (serviceType == typeof(CommandService) || serviceType == typeof(ICommandService) || serviceType == commandService.GetType())
                    return commandService;

                var service = provider.GetService(serviceType);
                if (!(service is null))
                    return service;

                throw new InvalidOperationException($"Failed to instantiate {type}, dependency of type {serviceType} was not found.");
            }

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
                throw new InvalidOperationException($"{type} has no public non-static constructors.");

            if (constructors.Length > 1)
                throw new InvalidOperationException($"{type} has multiple public constructors.");

            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            var properties = new List<PropertyInfo>();
            do
            {
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.SetMethod != null && !property.SetMethod.IsStatic && property.SetMethod.IsPublic
                        && property.GetCustomAttribute<DoNotAutomaticallyInjectAttribute>() == null)
                        properties.Add(property);
                }

                type = type.BaseType.GetTypeInfo();
            }
            while (type != typeof(object));

            return (provider) =>
            {
                for (var i = 0; i < parameters.Length; i++)
                    arguments[i] = GetDependency(provider, parameters[i].ParameterType);

                T instance;
                try
                {
                    instance = (T) constructor.Invoke(arguments);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to instantiate {type}. See the inner exception for more details.", ex);
                }

                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    property.SetValue(instance, GetDependency(provider, property.PropertyType));
                }

                return instance;
            };
        }

        private static readonly MethodInfo _getGenericTaskResultMethodInfo = typeof(ReflectionUtilities)
            .GetMethod(nameof(GetGenericTaskResult), BindingFlags.Static | BindingFlags.NonPublic);

        private static async Task<CommandResult> GetGenericTaskResult<T>(Task<T> task) where T : CommandResult
            => task != null ? await task.ConfigureAwait(false) as CommandResult : null;

        private static Func<object, object[], Task> CreateTaskDelegate(Type type, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            var instance = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object[]));
            var parameters = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(i)), methodParameters[i].ParameterType);

            var call = Expression.Call(Expression.Convert(instance, type), method, parameters);
            return method.ReturnType == typeof(Task)
                ? Expression.Lambda<Func<object, object[], Task>>(call, instance, arguments).Compile()
                : Expression.Lambda<Func<object, object[], Task>>(
                    Expression.Call(_getGenericTaskResultMethodInfo.MakeGenericMethod(method.ReturnType.GenericTypeArguments[0]), call), instance, arguments).Compile();
        }

#if NETCOREAPP
        private static readonly MethodInfo _getGenericValueTaskResultMethodInfo = typeof(ReflectionUtilities)
            .GetMethod(nameof(GetGenericValueTaskResult), BindingFlags.Static | BindingFlags.NonPublic);

        private static async ValueTask<CommandResult> GetGenericValueTaskResult<T>(ValueTask<T> task) where T : CommandResult
            => await task.ConfigureAwait(false);

        private static Func<object, object[], ValueTask<CommandResult>> CreateValueTaskDelegate(Type type, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            var instance = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object[]));
            var parameters = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(i)), methodParameters[i].ParameterType);

            var call = Expression.Call(Expression.Convert(instance, type), method, parameters);
            return Expression.Lambda<Func<object, object[], ValueTask<CommandResult>>>(
                    Expression.Call(_getGenericValueTaskResultMethodInfo.MakeGenericMethod(method.ReturnType.GenericTypeArguments[0]), call), instance, arguments).Compile();
        }
#endif

        public static CommandCallbackDelegate CreateCommandCallback(CommandService service, Type type, MethodInfo method)
        {
#if NETCOREAPP
            Func<object, object[], Task> taskDelegate = null;
            Func<object, object[], ValueTask<CommandResult>> valueTaskDelegate = null;
            if (method.ReturnType == typeof(Task) || method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                taskDelegate = CreateTaskDelegate(type, method);
            else
                valueTaskDelegate = CreateValueTaskDelegate(type, method);
#else
            var taskDelegate = CreateTaskDelegate(type, method);
#endif
            var constructor = CreateProviderConstructor<IModuleBase>(service, type);

            return async (context, provider) =>
            {
                var instance = constructor(provider);
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

#if NETCOREAPP
                    if (taskDelegate != null)
                    {
                        switch (taskDelegate(instance, context.InternalArguments))
                        {
                            case Task<CommandResult> genericTask:
                                return await genericTask.ConfigureAwait(false);

                            case Task task:
                                await task.ConfigureAwait(false);
                                break;
                        }
                    }
                    else
                    {
                        return await valueTaskDelegate(instance, context.InternalArguments).ConfigureAwait(false);
                    }
#else
                    switch (taskDelegate(instance, context.InternalArguments))
                    {
                        case Task<CommandResult> genericTask:
                            return await genericTask.ConfigureAwait(false);

                        case Task task:
                            await task.ConfigureAwait(false);
                            break;
                    }
#endif

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
                    }
                }
            };
        }

        public static readonly IReadOnlyDictionary<Type, Delegate> TryParseDelegates;

        public static IPrimitiveTypeParser CreatePrimitiveTypeParser(Type type)
            => Activator.CreateInstance(typeof(PrimitiveTypeParser<>).MakeGenericType(type)) as IPrimitiveTypeParser;

        public static IPrimitiveTypeParser CreateEnumTypeParser(Type type, Type enumType, bool ignoreCase)
            => (IPrimitiveTypeParser) typeof(EnumTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new object[] { enumType, ignoreCase });

        public static IPrimitiveTypeParser CreateNullableEnumTypeParser(Type type, IPrimitiveTypeParser enumTypeParser)
            => (IPrimitiveTypeParser) typeof(NullableEnumTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new[] { enumTypeParser });

        public static IPrimitiveTypeParser CreateNullablePrimitiveTypeParser(Type type, IPrimitiveTypeParser primitiveTypeParser)
            => (IPrimitiveTypeParser) typeof(NullablePrimitiveTypeParser<>).MakeGenericType(type).GetConstructors()[0].Invoke(new[] { primitiveTypeParser });

        public static ITypeParser CreateNullableTypeParser(Type nullableType, ITypeParser typeParser)
            => (ITypeParser) typeof(NullableTypeParser<>).MakeGenericType(nullableType).GetConstructors()[0].Invoke(new object[] { typeParser });

        static ReflectionUtilities()
        {
            TryParseDelegates = new Dictionary<Type, Delegate>(13)
            {
                [typeof(char)] =
#if NETCOREAPP
                    (TryParseDelegate<char>) TryParseChar,
#else
                    (TryParseDelegate<char>) char.TryParse,
#endif
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
#if NETCOREAPP
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
#endif
    }
}
