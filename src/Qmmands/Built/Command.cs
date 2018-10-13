using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a command built using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class Command
    {
        /// <summary>
        ///     Gets the name of this <see cref="Command"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this <see cref="Command"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the priority of this <see cref="Command"/>.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.RunMode"/> of this <see cref="Command"/>.
        /// </summary>
        public RunMode RunMode { get; }

        /// <summary>
        ///     Gets whether this <see cref="Command"/> ignores extra arguments or not.
        /// </summary>
        public bool IgnoreExtraArguments { get; }

        /// <summary>
        ///     Gets the aliases of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }

        /// <summary>
        ///     Gets the full aliases of this <see cref="Command"/>.
        /// </summary>
        /// <remarks>
        ///     Aliases of parent modules and this command concatenated using the <see cref="CommandService.Separator"/>.
        /// </remarks>
        public IReadOnlyList<string> FullAliases { get; }

        /// <summary>
        ///     Gets the checks of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<CheckBaseAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the parameters of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<Parameter> Parameters { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Module"/> of this <see cref="Command"/>.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        ///     Gets the callback of this <see cref="Command"/>.
        /// </summary>
        public CommandCallbackDelegate Callback { get; }

        internal CommandService Service => Module.Service;

        internal Command(CommandBuilder builder, Module module, bool userBuilt)
        {
            Module = module;

            Description = builder.Description;
            Priority = builder.Priority;
            RunMode = builder.RunMode ?? module.RunMode;
            IgnoreExtraArguments = builder.IgnoreExtraArguments ?? module.IgnoreExtraArguments;
            Aliases = builder.Aliases.AsReadOnly();

            if (Module.Aliases.Count == 0)
                FullAliases = builder.Aliases.AsReadOnly();

            else
            {
                var fullAliases = new List<string>();
                if (Module.FullAliases.Count > 0)
                {
                    for (var i = 0; i < Module.FullAliases.Count; i++)
                        for (var j = 0; j < Aliases.Count; j++)
                            fullAliases.Add(string.Concat(Module.FullAliases[i], Service.Separator, Aliases[j]));
                }

                else if (Aliases.Count == 0)
                    fullAliases.AddRange(Module.FullAliases);

                else
                    fullAliases.AddRange(Aliases);

                FullAliases = fullAliases.AsReadOnly();
            }

            Name = builder.Name ?? (FullAliases.Count > 0 ? FullAliases[0] : module.FullAliases.FirstOrDefault());

            Checks = builder.Checks.AsReadOnly();
            Attributes = builder.Attributes.AsReadOnly();

            var parameters = new List<Parameter>();
            for (var i = 0; i < builder.Parameters.Count; i++)
                parameters.Add(builder.Parameters[i].Build(this, userBuilt));
            Parameters = parameters.AsReadOnly();

            Callback = builder.Callback;
        }

        /// <summary>
        ///     Runs checks on parent modules and this command.
        /// </summary>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if all of the checks pass, otherwise a <see cref="ChecksFailedResult"/>.
        /// </returns>
        public async Task<IResult> RunChecksAsync(ICommandContext context, IServiceProvider provider = null)
        {
            if (provider is null)
                provider = EmptyServiceProvider.Instance;

            var result = await Module.RunChecksAsync(context, provider);
            if (!result.IsSuccessful)
                return result;

            if (Checks.Count > 0)
            {
                var checkResults = (await Task.WhenAll(Checks.Select(x => RunCheckAsync(x, context, provider))));
                if (!checkResults.GroupBy(x => x.Check.Group).Any(x => (x.Key == null && x.All(y => y.Error == null)) || (x.Key != null && x.Any(y => y.Error == null))))
                    return new ChecksFailedResult(this, checkResults.Where(x => x.Error != null).ToImmutableList());
            }

            return new SuccessfulResult();
        }

        private async Task<(CheckBaseAttribute Check, string Error)> RunCheckAsync(CheckBaseAttribute check, ICommandContext context, IServiceProvider provider)
        {
            var checkResult = await check.CheckAsync(context, provider);
            return (check, checkResult.Error);
        }

        /// <summary>
        ///     Returns this <see cref="Command"/>'s name or calls <see cref="object.ToString"/> if the name is null.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this command.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}