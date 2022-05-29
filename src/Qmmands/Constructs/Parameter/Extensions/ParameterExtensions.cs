using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Qommon;

namespace Qmmands;

public static class ParameterExtensions
{
    public static readonly ConditionalWeakTable<IParameter, ParameterTypeInformation> ParameterInformation = new();

    public class ParameterTypeInformation
    {
        public IParameter Parameter { get; }

        public bool IsOptional => Parameter.DefaultValue.HasValue || IsOptionalType;

        [MemberNotNullWhen(true, nameof(OptionalUnderlyingType))]
        public bool IsOptionalType => OptionalUnderlyingType != null;

        public Type? OptionalUnderlyingType { get; }

        public bool IsEnumerable { get; }

        public Type ActualType { get; }

        public bool IsStringLike => IsString || IsROM || IsMultiString;

        public bool IsString { get; }

        public bool IsROM { get; }

        public bool IsMultiString { get; }

        public bool IsNumeric => IsInteger || IsNumber;

        public bool IsInteger { get; }

        public bool IsNumber { get; }

        public bool AllowsNull { get; }

        private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

        public ParameterTypeInformation(IParameter parameter)
        {
            Parameter = parameter;

            var parameterInfo = parameter.ParameterInfo;
            NullabilityInfo? nullabilityInfo = null;
            if (parameterInfo != null)
            {
                lock (_nullabilityInfoContext)
                {
                    nullabilityInfo = _nullabilityInfoContext.Create(parameterInfo);
                }
            }

            var parameterType = parameter.ReflectedType;
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Optional<>))
            {
                OptionalUnderlyingType = parameterType.GenericTypeArguments[0];
                parameterType = OptionalUnderlyingType;
            }

            if (parameterType != typeof(string) && parameterType != typeof(MultiString) && parameterType.IsAssignableTo(typeof(IEnumerable)))
            {
                IsEnumerable = true;
            }

            Type actualType;
            if (IsEnumerable)
            {
                if (parameterType.IsArray)
                {
                    actualType = parameterType.GetElementType()!;
                    AllowsNull = nullabilityInfo == null || nullabilityInfo.ElementType!.WriteState != NullabilityState.NotNull;
                }
                else
                {
                    var interfaces = parameterType.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            actualType = @interface.GenericTypeArguments[0];
                            if (nullabilityInfo != null)
                            {
                                var index = Array.IndexOf(nullabilityInfo.GenericTypeArguments, actualType);
                                Guard.IsNotEqualTo(index, -1);
                                AllowsNull = nullabilityInfo.GenericTypeArguments[index].WriteState != NullabilityState.NotNull;
                            }
                            else
                            {
                                AllowsNull = true;
                            }

                            break;
                        }
                    }

                    actualType = typeof(object);
                    AllowsNull = true;
                }
            }
            else
            {
                actualType = parameterType;
                AllowsNull = nullabilityInfo == null || nullabilityInfo.WriteState != NullabilityState.NotNull;
            }

            if (actualType.TryGetNullableUnderlyingType(out var underlyingType))
            {
                actualType = underlyingType;
                IsString = false;
            }
            else
            {
                IsString = actualType == typeof(string);
            }

            IsROM = actualType == typeof(ReadOnlyMemory<char>);
            IsMultiString = actualType == typeof(MultiString);

            IsInteger = actualType == typeof(byte)
                || actualType == typeof(sbyte)
                || actualType == typeof(short)
                || actualType == typeof(ushort)
                || actualType == typeof(int)
                || actualType == typeof(uint)
                || actualType == typeof(long)
                || actualType == typeof(ulong)
                || actualType == typeof(nint)
                || actualType == typeof(nuint);

            IsNumber = actualType == typeof(Half)
                || actualType == typeof(float)
                || actualType == typeof(double)
                || actualType == typeof(decimal);

            ActualType = actualType;
        }

        public Type UnwrapOptional()
        {
            return IsOptionalType ? OptionalUnderlyingType : Parameter.ReflectedType;
        }
    }

    public static ParameterTypeInformation GetTypeInformation(this IParameter parameter)
    {
        return ParameterInformation.GetValue(parameter, parameter => new(parameter));
    }
}
