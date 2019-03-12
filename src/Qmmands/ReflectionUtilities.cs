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
        private static readonly TypeInfo _typeParserTypeInfo = typeof(ITypeParser).GetTypeInfo();
        private static readonly TypeInfo _moduleBaseTypeInfo = typeof(IModuleBase).GetTypeInfo();
        private static readonly TypeInfo _commandResultTypeInfo = typeof(CommandResult).GetTypeInfo();
        private static readonly TypeInfo _taskTypeInfo = typeof(Task).GetTypeInfo();
        private static readonly TypeInfo _objectTypeInfo = typeof(object).GetTypeInfo();
        private static readonly Type _nullableType = typeof(Nullable<>);
        private static readonly MethodInfo _getGenericTaskResultMethodInfo = typeof(ReflectionUtilities)
            .GetMethod(nameof(GetGenericTaskResult), BindingFlags.Static | BindingFlags.NonPublic);

        public static bool IsValidModuleDefinition(TypeInfo typeInfo)
            => _moduleBaseTypeInfo.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters;

        public static bool IsValidCommandDefinition(MethodInfo methodInfo)
            => methodInfo.IsPublic && !methodInfo.IsStatic && methodInfo.GetCustomAttribute<CommandAttribute>() != null
               && (methodInfo.ReturnType == _taskTypeInfo || methodInfo.ReturnType.IsConstructedGenericType
                                                             && methodInfo.ReturnType.GenericTypeArguments.Length == 1
                                                             && _commandResultTypeInfo.IsAssignableFrom(methodInfo.ReturnType.GenericTypeArguments[0]));

        public static bool IsValidParserDefinition(Type parserType, Type parameterType)
            => _typeParserTypeInfo.IsAssignableFrom(parserType) && !parserType.IsAbstract && parserType.BaseType.GetGenericArguments().Any(x => x == parameterType);

        public static bool IsNullable(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == _nullableType;

        public static Type MakeNullable(Type type)
            => _nullableType.MakeGenericType(type);

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

        public static ModuleBuilder BuildModule(CommandService service, TypeInfo typeInfo)
        {
            if (!IsValidModuleDefinition(typeInfo))
                throw new ArgumentException($"{typeInfo} must not be abstract, must not have generic parameters, and must inherit ModuleBase.", nameof(typeInfo));

            var builder = new ModuleBuilder(typeInfo);
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
                        builder.IgnoreExtraArguments = ignoreExtraArgumentsAttribute.IgnoreExtraArguments;
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
                builder.AddCommand(BuildCommand(service, typeInfo, command));

            foreach (var submodule in GetValidModules(typeInfo))
                builder.AddSubmodule(BuildModule(service, submodule));

            return builder;
        }

        public static CommandBuilder BuildCommand(CommandService service, TypeInfo typeInfo, MethodInfo methodInfo)
        {
            var builder = new CommandBuilder();
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
                        builder.WithIgnoreExtraArguments(ignoreExtraArgumentsAttribute.IgnoreExtraArguments);
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
                builder.AddParameters(BuildParameter(parameters[i], i + 1 == parameters.Length));

            builder.WithCallback(CreateCommandCallback(service, typeInfo, methodInfo));

            return builder;
        }

        public static ParameterBuilder BuildParameter(ParameterInfo parameterInfo, bool last)
        {
            var builder = new ParameterBuilder();
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
                        builder.CustomTypeParserType = overwriteTypeParserAttribute.CustomTypeParserType;

                        if (overwriteTypeParserAttribute.GetType() != typeof(OverrideTypeParserAttribute))
                            builder.AddAttribute(overwriteTypeParserAttribute);
                        break;

                    case ParameterCheckBaseAttribute parameterCheckAttribute:
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

        public static Func<IServiceProvider, T> CreateProviderConstructor<T>(CommandService commandService, TypeInfo typeInfo)
        {
            object GetDependency(IServiceProvider provider, Type type)
            {
                if (type == typeof(IServiceProvider) || type == provider.GetType())
                    return provider;

                if (type == typeof(CommandService) || type == typeof(ICommandService) || type == commandService.GetType())
                    return commandService;

                var service = provider.GetService(type);
                if (!(service is null))
                    return service;

                throw new InvalidOperationException($"Failed to instantiate {typeInfo}, dependency of type {type} was not found.");
            }

            var constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
                throw new InvalidOperationException($"{typeInfo} has no public non-static constructors.");

            if (constructors.Length > 1)
                throw new InvalidOperationException($"{typeInfo} has multiple public constructors.");

            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            var properties = new List<PropertyInfo>();
            do
            {
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (property.SetMethod != null && !property.SetMethod.IsStatic && property.SetMethod.IsPublic
                        && property.GetCustomAttribute<DoNotAutomaticallyInjectAttribute>() == null)
                        properties.Add(property);
                }

                typeInfo = typeInfo.BaseType.GetTypeInfo();
            }
            while (typeInfo != _objectTypeInfo);

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
                    throw new InvalidOperationException($"Failed to instantiate {typeInfo}. See the inner exception for more details.", ex);
                }

                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    property.SetValue(instance, GetDependency(provider, property.PropertyType));
                }

                return instance;
            };
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static async Task<CommandResult> GetGenericTaskResult<T>(Task<T> task)
            => task != null ? await task.ConfigureAwait(false) as CommandResult : null;
#pragma warning restore IDE0051 // Remove unused private members

        public static Func<object, object[], object> CreateExpressionDelegate(TypeInfo typeInfo, MethodInfo methodInfo)
        {
            var methodParameters = methodInfo.GetParameters();
            var instance = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object[]));
            var parameters = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var methodParam = methodParameters[i];
                parameters[i] = Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(i)), methodParam.ParameterType);
            }

            var call = Expression.Call(Expression.Convert(instance, typeInfo), methodInfo, parameters);
            return methodInfo.ReturnType == typeof(Task)
                ? Expression.Lambda<Func<object, object[], object>>(call, instance, arguments).Compile()
                : Expression.Lambda<Func<object, object[], object>>(Expression.Call(
                    _getGenericTaskResultMethodInfo.MakeGenericMethod(methodInfo.ReturnType.GenericTypeArguments[0]), call), instance, arguments).Compile();
        }

        public static CommandCallbackDelegate CreateCommandCallback(CommandService service, TypeInfo typeInfo, MethodInfo methodInfo)
        {
            var expressionDelegate = CreateExpressionDelegate(typeInfo, methodInfo);
            var constructor = CreateProviderConstructor<IModuleBase>(service, typeInfo);
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

                    switch (expressionDelegate(instance, context.InternalArguments))
                    {
                        case Task<CommandResult> genericTask:
                            return await genericTask.ConfigureAwait(false);

                        case Task task:
                            await task.ConfigureAwait(false);
                            break;
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
                [typeof(char)] = (TryParseDelegate<char>) char.TryParse,
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
    }
}
