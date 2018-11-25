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
        ///     Gets or sets the <see cref="System.Type"/> of the <see cref="Parameter"/>.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///     Gets or sets the custom <see cref="TypeParser{T}"/>'s type of the <see cref="Parameter"/>.
        /// </summary>
        public Type CustomTypeParserType { get; set; }

        /// <summary>
        ///     Gets the checks of the <see cref="Parameter"/>.
        /// </summary>
        public List<ParameterCheckBaseAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of the <see cref="Parameter"/>.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        ///     Initialises a new <see cref="ParameterBuilder"/>.
        /// </summary>
        public ParameterBuilder()
        {
            Checks = new List<ParameterCheckBaseAttribute>();
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
        ///     Sets the <see cref="Type"/>.
        /// </summary>
        public ParameterBuilder WithType(Type type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="CustomTypeParserType"/>.
        /// </summary>
        public ParameterBuilder WithCustomTypeParserType(Type customTypeParser)
        {
            CustomTypeParserType = customTypeParser;
            return this;
        }

        /// <summary>
        ///     Adds a check to <see cref="Checks"/>.
        /// </summary>
        public ParameterBuilder AddCheck(ParameterCheckBaseAttribute check)
        {
            Checks.Add(check);
            return this;
        }

        /// <summary>
        ///     Adds checks to <see cref="Checks"/>.
        /// </summary>
        public ParameterBuilder AddChecks(params ParameterCheckBaseAttribute[] checks)
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
            => new Parameter(this, command);
    }
}