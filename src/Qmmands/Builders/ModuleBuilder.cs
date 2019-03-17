﻿using System;
using System.Collections.Generic;

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
        ///     Gets the aliases of the <see cref="Module"/>.
        /// </summary>
        public List<string> Aliases { get; }

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
            Aliases = new List<string>();
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
        ///     Adds an alias to <see cref="Aliases"/>.
        /// </summary>
        public ModuleBuilder AddAlias(string alias)
        {
            Aliases.Add(alias);
            return this;
        }

        /// <summary>
        ///     Adds aliases to <see cref="Aliases"/>.
        /// </summary>
        public ModuleBuilder AddAliases(params string[] aliases)
        {
            Aliases.AddRange(aliases);
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
        ///     Adds checks to <see cref="Checks"/>.
        /// </summary>
        public ModuleBuilder AddChecks(params CheckAttribute[] checks)
        {
            Checks.AddRange(checks);
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
        ///     Adds attributes to <see cref="Attributes"/>.
        /// </summary>
        public ModuleBuilder AddAttributes(params Attribute[] attributes)
        {
            Attributes.AddRange(attributes);
            return this;
        }

        /// <summary>
        ///     Attempts to instantiate, modify, and add a <see cref="CommandBuilder"/> to <see cref="Commands"/>.
        /// </summary>
        /// <param name="callback"> The callback of the <see cref="Command"/>. </param>
        /// <param name="builderAction"> The action to perform on the builder. </param>
        public ModuleBuilder AddCommand(CommandCallbackDelegate callback, Action<CommandBuilder> builderAction)
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

        internal Module Build(CommandService service, Module parent)
        {
            var aliases = new List<string>(Aliases.Count);
            for (var i = 0; i < Aliases.Count; i++)
            {
                var alias = Aliases[i];
                if (alias == null)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain null entries.");

                if (aliases.Contains(alias))
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain duplicate.");

                if (alias.IndexOf(' ') != -1)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain whitespace.");

                if (alias.IndexOf(service.Separator, service.StringComparison) != -1)
                    throw new ModuleBuildingException(this, "Module's group aliases must not contain the separator.");

                aliases.Add(alias);
            }
            Aliases.Clear();
            for (var i = 0; i < aliases.Count; i++)
                Aliases.Add(aliases[i]);

            return new Module(service, this, parent);
        }
    }
}
