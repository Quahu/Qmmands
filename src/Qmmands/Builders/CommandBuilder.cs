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
        ///     Gets or sets whether the <see cref="Command"/> should ignore extra arguments or not.
        /// </summary>
        public bool? IgnoresExtraArguments { get; set; }

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
        ///     Gets the <see cref="Cooldown"/>s of the <see cref="Command"/>.
        /// </summary>
        public List<Cooldown> Cooldowns { get; }

        /// <summary>
        ///     Gets the aliases of the <see cref="Command"/>.
        /// </summary>
        public List<string> Aliases { get; }

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
        ///     Gets the module of the <see cref="Command"/>.
        /// </summary>
        public ModuleBuilder Module { get; }

        internal CommandCallbackDelegate Callback { get; }

        internal CommandBuilder(ModuleBuilder module, CommandCallbackDelegate callback)
        {
            Module = module;
            Callback = callback;
            Cooldowns = new List<Cooldown>();
            Aliases = new List<string>();
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
        ///     Sets the <see cref="IgnoresExtraArguments"/>.
        /// </summary>
        public CommandBuilder WithIgnoresExtraArguments(bool? ignoresExtraArguments)
        {
            IgnoresExtraArguments = ignoresExtraArguments;
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
        ///     Adds a <see cref="Cooldown"/> to <see cref="Cooldowns"/>.
        /// </summary>
        public CommandBuilder AddCooldown(Cooldown cooldown)
        {
            Cooldowns.Add(cooldown);
            return this;
        }

        /// <summary>
        ///     Adds <see cref="Cooldown"/>s to <see cref="Cooldowns"/>.
        /// </summary>
        public CommandBuilder AddCooldowns(params Cooldown[] cooldowns)
        {
            Cooldowns.AddRange(cooldowns);
            return this;
        }

        /// <summary>
        ///     Adds an alias to <see cref="Aliases"/>.
        /// </summary>
        public CommandBuilder AddAlias(string alias)
        {
            Aliases.Add(alias);
            return this;
        }

        /// <summary>
        ///     Adds aliases to <see cref="Aliases"/>.
        /// </summary>
        public CommandBuilder AddAliases(params string[] aliases)
        {
            Aliases.AddRange(aliases);
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
        ///     Adds checks to <see cref="Checks"/>.
        /// </summary>
        public CommandBuilder AddChecks(params CheckAttribute[] checks)
        {
            Checks.AddRange(checks);
            return this;
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="ParameterBuilder"/> to <see cref="Parameters"/>.
        /// </summary>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public CommandBuilder AddParameter(Action<ParameterBuilder> builderAction)
        {
            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            var builder = new ParameterBuilder(this);
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
        ///     Adds attributes to <see cref="Attributes"/>.
        /// </summary>
        public CommandBuilder AddAttributes(params Attribute[] attributes)
        {
            Attributes.AddRange(attributes);
            return this;
        }

        internal Command Build(Module module)
        {
            if (Callback is null)
                throw new CommandBuildingException(this, "Command's callback must not be null.");

            var aliases = new List<string>(Aliases.Count);
            for (var i = 0; i < Aliases.Count; i++)
            {
                var alias = Aliases[i];
                if (alias == null)
                    throw new CommandBuildingException(this, "Command's aliases must not contain null entries.");

                if (aliases.Contains(alias))
                    throw new CommandBuildingException(this, "Command's aliases must not contain duplicates.");

                if (alias.IndexOf(' ') != -1)
                    throw new CommandBuildingException(this, "Command's aliases must not contain whitespace.");

                if (alias.IndexOf(module.Service.Separator, module.Service.StringComparison) != -1)
                    throw new CommandBuildingException(this, "Command's aliases must not contain the separator.");

                aliases.Add(alias);
            }
            Aliases.Clear();
            for (var i = 0; i < aliases.Count; i++)
                Aliases.Add(aliases[i]);

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