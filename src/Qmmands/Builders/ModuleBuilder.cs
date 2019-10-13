using System;
using System.Collections.Generic;
using Qmmands.Delegates;

namespace Qmmands
{
    /// <summary>
    ///     Allows for building <see cref="Module"/>s using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class ModuleBuilder
    {
        /// <summary>
        ///     Gets or sets the name of the <see cref="Module"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the description of the <see cref="Module"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the remarks of the <see cref="Module"/>.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Qmmands.RunMode"/> of the <see cref="Module"/>.
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
        ///     Gets or sets whether the <see cref="Command"/>s in the <see cref="Module"/> should ignore extra arguments or not.
        /// </summary>
        public bool? IgnoresExtraArguments { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Type"/> of a custom <see cref="IArgumentParser"/>.
        /// </summary>
        public Type CustomArgumentParserType { get; set; }

        /// <summary>
        ///     Gets the aliases of the <see cref="Module"/>.
        /// </summary>
        public HashSet<string> Aliases { get; }

        /// <summary>
        ///     Gets the checks of the <see cref="Module"/>.
        /// </summary>
        public List<CheckAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of the <see cref="Module"/>.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the commands of the <see cref="Module"/>.
        /// </summary>
        public List<CommandBuilder> Commands { get; }

        /// <summary>
        ///     Gets the submodules of the <see cref="Module"/>.
        /// </summary>
        public List<ModuleBuilder> Submodules { get; }

        /// <summary>
        ///     Gets or sets whether the <see cref="Module"/> will be enabled or not.
        ///     Defaults to <see langword="true"/>.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        ///     Gets the parent module of the <see cref="Module"/>.
        /// </summary>
        public ModuleBuilder Parent { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> this <see cref="ModuleBuilder"/> was created from.
        ///     <see langword="null"/> if this module was created using, for example, <see cref="CommandService.AddModule(Type, Action{ModuleBuilder})"/>.
        /// </summary>
        public Type Type { get; }

        internal ModuleBuilder(ModuleBuilder parent)
        {
            Parent = parent;
            Aliases = new HashSet<string>();
            Checks = new List<CheckAttribute>();
            Attributes = new List<Attribute>();
            Commands = new List<CommandBuilder>();
            Submodules = new List<ModuleBuilder>();
        }

        internal ModuleBuilder(Type type, ModuleBuilder parent) : this(parent)
            => Type = type;

        /// <summary>
        ///     Sets the <see cref="Name"/>.
        /// </summary>
        public ModuleBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Description"/>.
        /// </summary>
        public ModuleBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="Remarks"/>.
        /// </summary>
        public ModuleBuilder WithRemarks(string remarks)
        {
            Remarks = remarks;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="RunMode"/>.
        /// </summary>
        public ModuleBuilder WithRunMode(RunMode? runMode)
        {
            RunMode = runMode;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IgnoresExtraArguments"/>.
        /// </summary>
        public ModuleBuilder WithIgnoresExtraArguments(bool? ignoresExtraArguments)
        {
            IgnoresExtraArguments = ignoresExtraArguments;
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="CustomArgumentParserType"/>.
        /// </summary>
        public ModuleBuilder WithCustomArgumentParserType(Type customArgumentParserType)
        {
            CustomArgumentParserType = customArgumentParserType;
            return this;
        }

        /// <summary>
        ///     Adds an alias to <see cref="Aliases"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     This alias has already been added.
        /// </exception>
        public ModuleBuilder AddAlias(string alias)
        {
            if (!Aliases.Add(alias))
                throw new ArgumentException($"This alias has already been added ({alias}).", nameof(alias));

            return this;
        }

        /// <summary>
        ///     Adds a check to <see cref="Checks"/>.
        /// </summary>
        public ModuleBuilder AddCheck(CheckAttribute check)
        {
            Checks.Add(check);
            return this;
        }

        /// <summary>
        ///     Adds an attribute to <see cref="Attributes"/>.
        /// </summary>
        public ModuleBuilder AddAttribute(Attribute attribute)
        {
            Attributes.Add(attribute);
            return this;
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(VoidCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(ResultCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(TaskCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(TaskResultCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(ValueTaskCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(ValueTaskResultCommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
            => AddCommandInternal(callback, builderAction);

        private ModuleBuilder AddCommandInternal(object callback, Action<CommandBuilder> builderAction)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            var builder = new CommandBuilder(this, callback);
            builderAction(builder);
            Commands.Add(builder);
            return this;
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="ModuleBuilder"/> to <see cref="Submodules"/>.
        /// </summary>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddSubmodule(Action<ModuleBuilder> builderAction)
        {
            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            var builder = new ModuleBuilder(this);
            builderAction(builder);
            Submodules.Add(builder);
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="IsEnabled"/>.
        /// </summary>
        public ModuleBuilder WithIsEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return this;
        }

        internal Module Build(CommandService service, Module parent)
        {
            if (CustomArgumentParserType != null && !Utilities.IsValidArgumentParserDefinition(CustomArgumentParserType))
                throw new ModuleBuildingException(this, $"{CustomArgumentParserType} is not a valid argument parser type.");

            foreach (var alias in Aliases)
            {
                if (alias == null)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain null entries.");

                if (alias.IndexOf(' ') != -1)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain whitespace.");

                if (alias.IndexOf(service.Separator, service.StringComparison) != -1)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain the separator.");
            }

            return new Module(service, this, parent);
        }
    }
}
