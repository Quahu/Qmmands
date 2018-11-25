using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
        public bool? IgnoreExtraArguments { get; set; }

        /// <summary>
        ///     Gets or sets the priority of the <see cref="Command"/>.
        /// </summary>
        public int Priority { get; set; }

        private RunMode? _runMode;

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

        /// <summary>
        ///     Gets or sets the callback of the <see cref="Command"/>.
        /// </summary>
        public CommandCallbackDelegate Callback { get; set; }

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
        public List<CheckBaseAttribute> Checks { get; }

        /// <summary>
        ///     Gets the custom attributes of the <see cref="Command"/>.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the parameters of the <see cref="Command"/>.
        /// </summary>
        public List<ParameterBuilder> Parameters { get; }

        /// <summary>
        ///     Initialises a new <see cref="CommandBuilder"/>.
        /// </summary>
        public CommandBuilder()
        {
            Cooldowns = new List<Cooldown>();
            Aliases = new List<string>();
            Checks = new List<CheckBaseAttribute>();
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
        ///     Sets the <see cref="IgnoreExtraArguments"/>.
        /// </summary>
        public CommandBuilder WithIgnoreExtraArguments(bool ignoreExtraArguments)
        {
            IgnoreExtraArguments = ignoreExtraArguments;
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
        ///     Sets the <see cref="Callback"/>.
        /// </summary>
        public CommandBuilder WithCallback(CommandCallbackDelegate callback)
        {
            Callback = callback;
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
        public CommandBuilder AddCheck(CheckBaseAttribute check)
        {
            Checks.Add(check);
            return this;
        }

        /// <summary>
        ///     Adds checks to <see cref="Checks"/>.
        /// </summary>
        public CommandBuilder AddChecks(params CheckBaseAttribute[] checks)
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
            var builder = new ParameterBuilder();
            builderAction(builder);
            Parameters.Add(builder);
            return this;
        }

        /// <summary>
        ///     Adds a parameter to <see cref="Parameters"/>.
        /// </summary>
        public CommandBuilder AddParameter(ParameterBuilder parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        ///     Adds parameters to <see cref="Parameters"/>.
        /// </summary>
        public CommandBuilder AddParameters(params ParameterBuilder[] parameters)
        {
            Parameters.AddRange(parameters);
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
            if (module is null)
                throw new ArgumentNullException(nameof(module), "Command's module mustn't be null.");

            if (Callback is null)
                throw new InvalidOperationException("Command's callback mustn't be null.");

            var aliases = Aliases.ToImmutableArray();
            Aliases.Clear();
            for (var i = 0; i < aliases.Length; i++)
            {
                var alias = aliases[i];
                if (alias == null)
                    continue;

                if (!Aliases.Contains(aliases[i]))
                {
                    if (alias.IndexOf(' ') != -1)
                        throw new InvalidOperationException($"Command's aliases mustn't contain whitespace ({alias}).");

                    if (alias.IndexOf(module.Service.Separator) != -1)
                        throw new InvalidOperationException($"Command's aliases mustn't contain the separator ({alias}).");

                    Aliases.Add(alias.Trim());
                }
            }

            ParameterBuilder previous = null;
            for (var i = 0; i < Parameters.Count; i++)
            {
                var current = Parameters[i];
                if (previous != null && previous.IsOptional && !current.IsOptional)
                    throw new InvalidOperationException("Optional parameters mustn't appear before required ones.");

                previous = current;
            }

            return new Command(this, module);
        }
    }
}