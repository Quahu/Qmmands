using System;
using System.Collections.Generic;

namespace Qmmands
{
    /// <summary>
    ///     Allows for building <see cref="Command"/>s using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class CommandBuilder
    {
        /// <summary>
        ///     Gets or sets the name of the <see cref="Command"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the description of the <see cref="Command"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the remarks of the <see cref="Command"/>.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        ///     Gets or sets the priority of the <see cref="Command"/>.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Qmmands.RunMode"/> of the <see cref="Command"/>.
        /// </summary>
        public RunMode? RunMode
        {
            get => _runMode;
            set
            {
                if (value != null && !Enum.IsDefined(typeof(RunMode), value.Value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Invalid run mode.");

                _runMode = value;
            }
        }
        private RunMode? _runMode;

        /// <summary>
        ///     Gets or sets whether the <see cref="Command"/> should ignore extra arguments or not.
        /// </summary>
        public bool? IgnoresExtraArguments { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Type"/> of a custom <see cref="IArgumentParser"/>.
        /// </summary>
        public Type CustomArgumentParserType { get; set; }

        /// <summary>
        ///     Gets the <see cref="Cooldown"/>s of the <see cref="Command"/>.
        /// </summary>
        public List<Cooldown> Cooldowns { get; }

        /// <summary>
        ///     Gets the aliases of the <see cref="Command"/>.
        /// </summary>
        public HashSet<string> Aliases { get; }

        /// <summary>
        ///     Gets the checks of the <see cref="Command"/>.
        /// </summary>
        public List<CheckAttribute> Checks { get; }

        /// <summary>
        ///     Gets the custom attributes of the <see cref="Command"/>.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the parameters of the <see cref="Command"/>.
        /// </summary>
        public List<ParameterBuilder> Parameters { get; }

        /// <summary>
        ///     Gets or sets whether the <see cref="Command"/> will be enabled or not.
        ///     Defaults to <see langword="true"/>.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        ///     Gets the module of the <see cref="Command"/>.
        /// </summary>
        public ModuleBuilder Module { get; }

        internal readonly object Callback;

        internal CommandBuilder(ModuleBuilder module, object callback)
        {
            Module = module;
            Callback = callback;
            Cooldowns = new List<Cooldown>();
            Aliases = new HashSet<string>(1);
            Checks = new List<CheckAttribute>();
            Attributes = new List<Attribute>();
            Parameters = new List<ParameterBuilder>();
        }

        /// <summary>
        ///     Sets the <see cref="Name"/>.
        /// </summary>
        public CommandBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Description"/>.
        /// </summary>
        public CommandBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Remarks"/>.
        /// </summary>
        public CommandBuilder WithRemarks(string remarks)
        {
            Remarks = remarks;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Priority"/>.
        /// </summary>
        public CommandBuilder WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="RunMode"/>.
        /// </summary>
        public CommandBuilder WithRunMode(RunMode? runMode)
        {
            RunMode = runMode;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IgnoresExtraArguments"/>.
        /// </summary>
        public CommandBuilder WithIgnoresExtraArguments(bool? ignoresExtraArguments)
        {
            IgnoresExtraArguments = ignoresExtraArguments;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="CustomArgumentParserType"/>.
        /// </summary>
        public CommandBuilder WithCustomArgumentParserType(Type customArgumentParserType)
        {
            CustomArgumentParserType = customArgumentParserType;
            return this;
        }

        /// <summary>
        ///     Adds a <see cref="Cooldown"/> to <see cref="Cooldowns"/>.
        /// </summary>
        public CommandBuilder AddCooldown(Cooldown cooldown)
        {
            Cooldowns.Add(cooldown);
            return this;
        }

        /// <summary>
        ///     Adds an alias to <see cref="Aliases"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     This alias has already been added.
        /// </exception>
        public CommandBuilder AddAlias(string alias)
        {
            if (!Aliases.Add(alias))
                throw new ArgumentException($"This alias has already been added ({alias}).", nameof(alias));

            return this;
        }

        /// <summary>
        ///     Adds a check to <see cref="Checks"/>.
        /// </summary>
        public CommandBuilder AddCheck(CheckAttribute check)
        {
            Checks.Add(check);
            return this;
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="ParameterBuilder"/> to <see cref="Parameters"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> of the <see cref="Parameter"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public CommandBuilder AddParameter(Type type, Action<ParameterBuilder> builderAction)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "Parameter's type must not be null.");

            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            var builder = new ParameterBuilder(type, this);
            builderAction(builder);
            Parameters.Add(builder);
            return this;
        }

        /// <summary>
        ///     Adds an attribute to <see cref="Attributes"/>.
        /// </summary>
        public CommandBuilder AddAttribute(Attribute attribute)
        {
            Attributes.Add(attribute);
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IsEnabled"/>.
        /// </summary>
        public CommandBuilder WithIsEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return this;
        }

        internal Command Build(Module module)
        {
            if (CustomArgumentParserType != null && !Utilities.IsValidArgumentParserDefinition(CustomArgumentParserType))
                throw new CommandBuildingException(this, $"{CustomArgumentParserType} is not a valid argument parser type.");

            foreach (var alias in Aliases)
            {
                if (alias == null)
                    throw new CommandBuildingException(this, "Command's aliases must not contain null entries.");

                if (alias.IndexOf(' ') != -1)
                    throw new CommandBuildingException(this, "Command's aliases must not contain whitespace.");

                if (alias.IndexOf(module.Service.Separator, module.Service.StringComparison) != -1)
                    throw new CommandBuildingException(this, "Command's aliases must not contain the separator.");
            }

            ParameterBuilder previous = null;
            for (var i = 0; i < Parameters.Count; i++)
            {
                var current = Parameters[i];
                if (previous != null && previous.IsOptional && !current.IsOptional)
                    throw new CommandBuildingException(this, "Optional parameters must not appear before required ones.");

                previous = current;
            }

            return new Command(this, module);
        }
    }
}