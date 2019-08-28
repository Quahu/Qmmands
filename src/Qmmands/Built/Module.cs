using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a module built using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class Module
    {
        /// <summary>
        ///     Gets the name of this <see cref="Module"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this <see cref="Module"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the remarks of this <see cref="Module"/>.
        /// </summary>
        public string Remarks { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.RunMode"/> of this <see cref="Module"/>.
        /// </summary>
        public RunMode RunMode { get; }

        /// <summary>
        ///     Gets whether this <see cref="Module"/>'s commands ignore extra arguments or not.
        /// </summary>
        public bool IgnoresExtraArguments { get; }

        /// <summary>
        ///     Gets or sets the <see cref="Type"/> of a custom <see cref="IArgumentParser"/>.
        /// </summary>
        public Type CustomArgumentParserType { get; }

        /// <summary>
        ///     Gets the aliases of this <see cref="Module"/>.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }

        /// <summary>
        ///     Gets the full aliases of this <see cref="Module"/>.
        /// </summary>
        /// <remarks>
        ///     Aliases of parent <see cref="Module"/>s and this <see cref="Module"/> concatenated using the <see cref="CommandService.Separator"/>.
        /// </remarks>
        public IReadOnlyList<string> FullAliases { get; }

        /// <summary>
        ///     Gets the checks of this <see cref="Module"/>.
        /// </summary>
        public IReadOnlyList<CheckAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of this <see cref="Module"/>.
        /// </summary>
        public IReadOnlyList<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the submodules of this <see cref="Module"/>.
        /// </summary>
        public IReadOnlyList<Module> Submodules { get; }

        /// <summary>
        ///     Gets the commands of this <see cref="Module"/>.
        /// </summary>
        public IReadOnlyList<Command> Commands { get; }

        /// <summary>
        ///     Gets whether this <see cref="Module"/> is enabled or not.
        /// </summary>
        public bool IsEnabled => Parent != null
            ? Parent.IsEnabled && Volatile.Read(ref _isEnabled)
            : Volatile.Read(ref _isEnabled);

        /// <summary>
        ///     Gets the parent <see cref="Module"/> of this <see cref="Module"/>.
        /// </summary>
        public Module Parent { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> this <see cref="Module"/> was built from.
        ///     <see langword="null"/> if it was built using a <see cref="ModuleBuilder"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the <see cref="CommandService"/> of this <see cref="Module"/>.
        /// </summary>
        public CommandService Service { get; }

        private bool _isEnabled;

        internal Module(CommandService service, ModuleBuilder builder, Module parent)
        {
            Parent = parent;
            Type = builder.Type;
            Service = service;

            Description = builder.Description;
            Remarks = builder.Remarks;
            RunMode = builder.RunMode ?? Parent?.RunMode ?? Service.DefaultRunMode;
            IgnoresExtraArguments = builder.IgnoresExtraArguments ?? Parent?.IgnoresExtraArguments ?? Service.IgnoresExtraArguments;
            CustomArgumentParserType = builder.CustomArgumentParserType ?? Parent?.CustomArgumentParserType;
            var aliases = builder.Aliases.ToImmutableArray();
            Aliases = aliases;

            var fullAliases = ImmutableArray.CreateBuilder<string>();
            if (Parent == null || Parent.FullAliases.Count == 0)
            {
                fullAliases.AddRange(aliases);
            }
            else if (aliases.Length == 0)
            {
                fullAliases.AddRange((ImmutableArray<string>) Parent.FullAliases);
            }
            else
            {
                for (var i = 0; i < Parent.FullAliases.Count; i++)
                {
                    var parentAlias = Parent.FullAliases[i];
                    var absolute = parentAlias.Length == 0;
                    for (var j = 0; j < aliases.Length; j++)
                    {
                        var alias = aliases[j];
                        if (alias.Length == 0)
                        {
                            if (absolute)
                                continue;

                            fullAliases.Add(parentAlias);
                        }
                        else if (absolute)
                        {
                            fullAliases.Add(alias);
                        }
                        else
                        {
                            fullAliases.Add(string.Concat(parentAlias, Service.Separator, alias));
                        }
                    }
                }
            }
            FullAliases = fullAliases.TryMoveToImmutable();

            Name = builder.Name ?? Type?.Name;

            for (var i = 0; i < builder.Checks.Count; i++)
                builder.Checks[i].Module = this;
            Checks = builder.Checks.ToImmutableArray();
            Attributes = builder.Attributes.ToImmutableArray();

            var modules = ImmutableArray.CreateBuilder<Module>(builder.Submodules.Count);
            for (var i = 0; i < builder.Submodules.Count; i++)
                modules.Add(builder.Submodules[i].Build(service, this));
            Submodules = modules.TryMoveToImmutable();

            var commands = ImmutableArray.CreateBuilder<Command>(builder.Commands.Count);
            for (var i = 0; i < builder.Commands.Count; i++)
                commands.Add(builder.Commands[i].Build(this));
            Commands = commands.TryMoveToImmutable();

            _isEnabled = builder.IsEnabled;
        }

        /// <summary>
        ///     Runs checks on parent <see cref="Module"/>s and this <see cref="Module"/>.
        /// </summary>
        /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if all of the checks pass, otherwise a <see cref="ChecksFailedResult"/>.
        /// </returns>
        public async Task<IResult> RunChecksAsync(CommandContext context)
        {
            if (Parent != null)
            {
                var result = await Parent.RunChecksAsync(context).ConfigureAwait(false);
                if (!result.IsSuccessful)
                    return result;
            }

            if (Checks.Count > 0)
            {
                async Task<(CheckAttribute Check, CheckResult Result)> RunCheckAsync(CheckAttribute check)
                {
                    var checkResult = await check.CheckAsync(context).ConfigureAwait(false);
                    return (check, checkResult);
                }

                var checkResults = await Task.WhenAll(Checks.Select(RunCheckAsync)).ConfigureAwait(false);
                var failedGroups = checkResults.GroupBy(x => x.Check.Group)
                    .Where(x => x.Key == null ? x.Any(y => !y.Result.IsSuccessful) : x.All(y => !y.Result.IsSuccessful)).ToImmutableArray();
                if (failedGroups.Length > 0)
                    return new ChecksFailedResult(this, failedGroups.SelectMany(x => x).Where(x => !x.Result.IsSuccessful).ToImmutableArray());
            }

            return new SuccessfulResult();
        }

        /// <summary>
        ///     Enables this <see cref="Module"/>.
        /// </summary>
        public void Enable()
            => Volatile.Write(ref _isEnabled, true);

        /// <summary>
        ///     Disables this <see cref="Module"/>.
        /// </summary>
        public void Disable()
            => Volatile.Write(ref _isEnabled, false);

        /// <summary>
        ///     Returns <see cref="Name"/> or calls <see cref="object.ToString"/> if it is <see langword="null"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this <see cref="Module"/>.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}
