using System;
using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Allows for building <see cref="Parameter"/>s using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class ParameterBuilder
    {
        /// <summary>
        ///     Gets or sets the name of the <see cref="Parameter"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the description of the <see cref="Parameter"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the remarks of the <see cref="Parameter"/>.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        ///     Gets or sets whether the <see cref="Parameter"/> is multiple or not.
        /// </summary>
        public bool IsMultiple { get; set; }

        /// <summary>
        ///     Gets or sets whether the <see cref="Parameter"/> is optional or not.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        ///     Gets or sets whether the <see cref="Parameter"/> is a remainder parameter or not.
        /// </summary>
        public bool IsRemainder { get; set; }

        /// <summary>
        ///     Gets or sets the default value of the <see cref="Parameter"/>.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="System.Type"/> of a custom <see cref="TypeParser{T}"/>.
        /// </summary>
        public Type CustomTypeParserType { get; set; }

        /// <summary>
        ///     Gets the checks of the <see cref="Parameter"/>.
        /// </summary>
        public List<ParameterCheckAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of the <see cref="Parameter"/>.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> of the <see cref="Parameter"/>.
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        ///     Gets the command of the <see cref="Parameter"/>.
        /// </summary>
        public CommandBuilder Command { get; }

        internal ParameterBuilder(Type type, CommandBuilder command)
        {
            Type = type;
            Command = command;
            Checks = new List<ParameterCheckAttribute>();
            Attributes = new List<Attribute>();
        }

        /// <summary>
        ///     Sets the <see cref="Name"/>.
        /// </summary>
        public ParameterBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Description"/>.
        /// </summary>
        public ParameterBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Remarks"/>.
        /// </summary>
        public ParameterBuilder WithRemarks(string remarks)
        {
            Remarks = remarks;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IsMultiple"/>.
        /// </summary>
        public ParameterBuilder WithIsMultiple(bool isMultiple)
        {
            IsMultiple = isMultiple;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IsOptional"/>.
        /// </summary>
        public ParameterBuilder WithIsOptional(bool isOptional)
        {
            IsOptional = isOptional;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IsRemainder"/>.
        /// </summary>
        public ParameterBuilder WithIsRemainder(bool isRemainder)
        {
            IsRemainder = isRemainder;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="DefaultValue"/>.
        /// </summary>
        public ParameterBuilder WithDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="CustomTypeParserType"/>.
        /// </summary>
        public ParameterBuilder WithCustomTypeParserType(Type customTypeParserType)
        {
            CustomTypeParserType = customTypeParserType;
            return this;
        }

        /// <summary>
        ///     Adds a check to <see cref="Checks"/>.
        /// </summary>
        public ParameterBuilder AddCheck(ParameterCheckAttribute check)
        {
            Checks.Add(check);
            return this;
        }

        /// <summary>
        ///     Adds checks to <see cref="Checks"/>.
        /// </summary>
        public ParameterBuilder AddChecks(params ParameterCheckAttribute[] checks)
        {
            Checks.AddRange(checks);
            return this;
        }

        /// <summary>
        ///     Adds an attribute to <see cref="Attributes"/>.
        /// </summary>
        public ParameterBuilder AddAttribute(Attribute attribute)
        {
            Attributes.Add(attribute);
            return this;
        }

        /// <summary>
        ///     Adds attributes to <see cref="Attributes"/>.
        /// </summary>
        public ParameterBuilder AddAttributes(params Attribute[] attributes)
        {
            Attributes.AddRange(attributes);
            return this;
        }

        internal Parameter Build(Command command)
        {
            if (CustomTypeParserType != null && !Utilities.IsValidTypeParserDefinition(CustomTypeParserType, Type))
                throw new ParameterBuildingException(this, $"{CustomTypeParserType} is not a valid type parser for parameter of type {Type}.");

            if (IsOptional)
            {
                if (DefaultValue == null)
                {
                    if (Type.IsValueType && !Utilities.IsNullable(Type))
                        throw new ParameterBuildingException(this, "Value type parameter's default value must not be null.");
                }
                else if (DefaultValue.GetType() != (IsMultiple ? Type.MakeArrayType() : Type))
                {
                    throw new ParameterBuildingException(this, $"Parameter type and default value mismatch. Expected {Type}, got {DefaultValue.GetType()}.");
                }
            }

            return new Parameter(this, command);
        }
    }
}