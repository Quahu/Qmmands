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
    /// <summary>
    ///     Represents the cache for <see cref="ParameterTypeInformation"/>.
    /// </summary>
    public static readonly ConditionalWeakTable<IParameter, ParameterTypeInformation> ParameterInformation = new();

    /// <summary>
    ///     Represents the type information of a parameter.
    /// </summary>
    public class ParameterTypeInformation
    {
        /// <summary>
        ///     Gets the parameter this instance is for.
        /// </summary>
        public IParameter Parameter { get; }

        /// <summary>
        ///     Gets whether the parameter is optional.
        /// </summary>
        public bool IsOptional => Parameter.DefaultValue.HasValue || IsOptionalType;

        /// <summary>
        ///     Gets whether the parameter is an <see cref="Optional{T}"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(OptionalUnderlyingType))]
        public bool IsOptionalType => OptionalUnderlyingType != null;

        /// <summary>
        ///     Gets the underlying type of the <see cref="Optional{T}"/>.
        /// </summary>
        public Type? OptionalUnderlyingType { get; }

        /// <summary>
        ///     Gets whether the parameter is an enumerable.
        /// </summary>
        public bool IsEnumerable { get; }

        /// <summary>
        ///     Gets the actual type of the parameter.
        /// </summary>
        public Type ActualType { get; }

        /// <summary>
        ///     Gets whether the parameter is string-like,
        ///     i.e. a <see cref="string"/>, <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/>, or <see cref="MultiString"/>.
        /// </summary>
        public bool IsStringLike => IsString || IsROM || IsMultiString;

        /// <summary>
        ///     Gets whether the parameter is a <see cref="string"/>.
        /// </summary>
        public bool IsString { get; }

        /// <summary>
        ///     Gets whether the parameter is a <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/>.
        /// </summary>
        public bool IsROM { get; }

        /// <summary>
        ///     Gets whether the parameter is a <see cref="MultiString"/>.
        /// </summary>
        public bool IsMultiString { get; }

        /// <summary>
        ///     Gets whether the parameter is an integer or a number.
        /// </summary>
        public bool IsNumeric => IsInteger || IsNumber;

        /// <summary>
        ///     Gets whether the parameter is an integer.
        /// </summary>
        public bool IsInteger { get; }

        /// <summary>
        ///     Gets whether the parameter is a number.
        /// </summary>
        public bool IsNumber { get; }

        /// <summary>
        ///     Gets whether the parameter is a <see cref="bool"/>.
        /// </summary>
        public bool IsBoolean { get; }

        /// <summary>
        ///     Gets whether the parameter allows <see langword="null"/>.
        /// </summary>
        public bool AllowsNull { get; }

        private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

        /// <summary>
        ///     Instantiates a new <see cref="ParameterTypeInformation"/> for the given parameter.
        /// </summary>
        /// <param name="parameter"> The parameter to get the information for. </param>
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
                    Type? innerType = null;
                    var interfaces = parameterType.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            innerType = @interface.GenericTypeArguments[0];
                            break;
                        }
                    }

                    if (innerType != null)
                    {
                        actualType = innerType;

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
                    }
                    else
                    {
                        actualType = typeof(object);
                        AllowsNull = true;
                    }
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

            if (!IsString)
                IsROM = actualType == typeof(ReadOnlyMemory<char>);

            if (!IsString && !IsROM)
                IsMultiString = actualType == typeof(MultiString);

            if (!IsStringLike)
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

            if (!IsStringLike && !IsInteger)
                IsNumber = actualType == typeof(Half)
                    || actualType == typeof(float)
                    || actualType == typeof(double)
                    || actualType == typeof(decimal);

            if (!IsStringLike && !IsInteger && !IsNumber)
                IsBoolean = actualType == typeof(bool);

            ActualType = actualType;
        }

        public Type UnwrapOptional()
        {
            return IsOptionalType ? OptionalUnderlyingType : Parameter.ReflectedType;
        }
    }

    /// <summary>
    ///     Gets the type information of this parameter.
    /// </summary>
    /// <param name="parameter"> This parameter. </param>
    /// <returns>
    ///     The type information of this parameter.
    /// </returns>
    public static ParameterTypeInformation GetTypeInformation(this IParameter parameter)
    {
        return ParameterInformation.GetValue(parameter, parameter => new(parameter));
    }
}
