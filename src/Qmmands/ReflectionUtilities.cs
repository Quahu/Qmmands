using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<Type, Func<Task, IResult>> _commandResults =
            new ConcurrentDictionary<Type, Func<Task, IResult>> { [typeof(Task)] = x => new SuccessfulResult() };

        public static bool IsValidModuleDefinition(TypeInfo typeInfo)
            => _moduleBaseTypeInfo.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters;

        public static bool IsValidCommandDefinition(MethodInfo methodInfo)
            => methodInfo.IsPublic && methodInfo.GetCustomAttribute<CommandAttribute>() != null
               && (methodInfo.ReturnType == _taskTypeInfo || (methodInfo.ReturnType.IsConstructedGenericType
                                                             && methodInfo.ReturnType.GenericTypeArguments.Length == 1
                                                             && _commandResultTypeInfo.IsAssignableFrom(methodInfo.ReturnType.GenericTypeArguments[0])));

        public static bool IsValidParserDefinition(Type typeInfo, Type parameterType)
            => _typeParserTypeInfo.IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract && typeInfo.BaseType.GetGenericArguments().Any(x => x == parameterType);

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

        public static ModuleBuilder BuildModule(TypeInfo typeInfo)
        {
            if (!IsValidModuleDefinition(typeInfo))
                throw new ArgumentException($"{typeInfo.Name} mustn't be abstract, have generic parameters, and must inherit ModuleBase.", nameof(typeInfo));

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

                    case CheckBaseAttribute checkAttribute:
                        builder.AddCheck(checkAttribute);
                        break;

                    case Attribute attribute:
                        builder.AddAttribute(attribute);
                        break;
                }
            }

            foreach (var command in GetValidCommands(typeInfo))
                builder.AddCommand(BuildCommand(typeInfo, command));

            foreach (var submodule in GetValidModules(typeInfo))
                builder.AddSubmodule(BuildModule(submodule));

            return builder;
        }

        public static CommandBuilder BuildCommand(TypeInfo typeInfo, MethodInfo methodInfo)
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

                    case CheckBaseAttribute checkAttribute:
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

            builder.WithCallback(CreateCommandCallback(typeInfo, methodInfo));

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
                            throw new InvalidOperationException($"A params array parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType.Name}.");

                        builder.WithIsMultiple(true)
                            .WithType(parameterInfo.ParameterType.GetElementType());
                        break;

                    case RemainderAttribute _:
                        if (!last)
                            throw new InvalidOperationException($"A remainder parameter must be the last parameter in a command. Parameter: {parameterInfo.Name} in {parameterInfo.Member.Name} in {parameterInfo.Member.DeclaringType.Name}.");

                        builder.WithIsRemainder(true);
                        break;

                    case OverwriteTypeParserAttribute overwriteTypeParserAttribute:
                        builder.CustomTypeParserType = overwriteTypeParserAttribute.CustomTypeParserType;
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

                if (type == typeof(CommandService))
                    return commandService;

                var service = provider.GetService(type);
                if (!(service is null))
                    return service;

                throw new InvalidOperationException($"Failed to create {typeInfo}, dependency {type.Name} wasn't found.");
            }

            IEnumerable<PropertyInfo> GetInjectableProperties(TypeInfo type)
            {
                do
                {
                    foreach (var property in type.DeclaredProperties)
                    {
                        if (property.SetMethod != null && !property.SetMethod.IsStatic && property.SetMethod.IsPublic
                            && property.GetCustomAttribute<DontAutoInjectAttribute>() == null)
                            yield return property;
                    }

                    type = type.BaseType.GetTypeInfo();
                }
                while (type != _objectTypeInfo);
            }

            return (provider) =>
            {
                var constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                if (constructors.Length == 0)
                    throw new InvalidOperationException($"{typeInfo.Name} has no public non-static constructors.");

                if (constructors.Length > 1)
                    throw new InvalidOperationException($"{typeInfo.Name} has multiple public constructors.");

                var constructor = constructors[0];
                var parameters = constructor.GetParameters();
                var arguments = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                    arguments[i] = GetDependency(provider, parameters[i].ParameterType);

                T instance;
                try
                {
                    instance = (T) constructor.Invoke(arguments);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to instantiate {typeInfo.Name}. See the inner exception for more details.", ex);
                }

                foreach (var property in GetInjectableProperties(typeInfo))
                    property.SetValue(instance, GetDependency(provider, property.PropertyType));

                return instance;
            };
        }

        public static CommandCallbackDelegate CreateCommandCallback(TypeInfo typeInfo, MethodInfo methodInfo)
            => async (command, arguments, context, provider) =>
            {
                var instance = CreateProviderConstructor<IModuleBase>(command.Service, typeInfo)(provider);
                instance.Prepare(context);

                var executeAfter = true;
                try
                {
                    try
                    {
                        await instance.BeforeExecutedAsync(command).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        executeAfter = false;
                        return new ExecutionFailedResult(command, CommandExecutionStep.BeforeExecuted, ex);
                    }

                    if (!(methodInfo.Invoke(instance, arguments) is Task task))
                        return new SuccessfulResult();

                    var resultFunc = _commandResults.GetOrAdd(methodInfo.ReturnType, _ =>
                        {
                            var taskParameter = Expression.Parameter(typeof(Task));
                            return Expression.Lambda<Func<Task, IResult>>(
                                Expression.Property(Expression.Convert(taskParameter, methodInfo.ReturnType), "Result"), taskParameter).Compile();
                        });

                    await task.ConfigureAwait(false);
                    return resultFunc(task);
                }
                finally
                {
                    try
                    {
                        if (executeAfter)
                            await instance.AfterExecutedAsync(command).ConfigureAwait(false);
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
}
