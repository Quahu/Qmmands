using System;
using System.Collections.Generic;
using System.Reflection;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter built using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class Parameter
    {
        /// <summary>
        ///     Gets the name of this <see cref="Parameter"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this <see cref="Parameter"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is a remainder parameter or not.
        /// </summary>
        public bool IsRemainder { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is multiple or not.
        /// </summary>
        public bool IsMultiple { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is optional or not.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        ///     Gets the default value of this <see cref="Parameter"/>.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> of this <see cref="Parameter"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the custom <see cref="TypeParser{T}"/>'s type of this <see cref="Parameter"/>.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Gets the checks of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<ParameterCheckBaseAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the command of this <see cref="Parameter"/>.
        /// </summary>
        public Command Command { get; }

        internal Parameter(ParameterBuilder builder, Command command, bool userBuilt)
        {
            Name = builder.Name;
            Description = builder.Description;
            IsRemainder = builder.IsRemainder;
            IsMultiple = builder.IsMultiple;
            IsOptional = builder.IsOptional;
            DefaultValue = builder.DefaultValue;
            Type = builder.Type;

            if (userBuilt)
            {
                if (Type == null)
                    throw new InvalidOperationException("Parameter must have an assigned type.");

                if (IsOptional)
                {
                    if (DefaultValue is null)
                    {
                        if (Type.IsValueType && Type.GenericTypeArguments.Length > 0 && Type.GenericTypeArguments[0].IsValueType && Type != (typeof(Nullable<>).MakeGenericType(Type.GenericTypeArguments[0])))
                            throw new InvalidOperationException(
                                "A value type parameter can't have null as the default value.");
                    }

                    else if (DefaultValue.GetType() != Type)
                        throw new InvalidOperationException(
                            $"Parameter type and default value mismatch. Expected {Type.Name}, got {DefaultValue.GetType().Name}.");
                }
            }

            if (builder.CustomTypeParserType != null)
            {
                if (!ReflectionUtils.IsValidParserDefinition(builder.CustomTypeParserType.GetTypeInfo(), Type))
                    throw new InvalidOperationException($"{builder.CustomTypeParserType.Name} isn't a valid type parser for {this} ({Type.Name}).");

                CustomTypeParserType = builder.CustomTypeParserType;
            }
            Checks = builder.Checks.AsReadOnly();
            Attributes = builder.Attributes.AsReadOnly();

            Command = command;
        }

        /// <summary>
        ///     Returns this <see cref="Parameter"/>'s name or calls <see cref="object.ToString"/> if the name is null.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this command.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}
