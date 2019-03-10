using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
        ///     Gets the remarks of this <see cref="Command"/>.
        /// </summary>
        public string Remarks { get; }

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
        ///     Gets the callback of this <see cref="Command"/>.
        /// </summary>
        public CommandCallbackDelegate Callback { get; }

        /// <summary>
        ///     Gets the <see cref="Cooldown"/>s of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<Cooldown> Cooldowns { get; }

        /// <summary>
        ///     Gets the aliases of this <see cref="Command"/>.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }

        /// <summary>
        ///     Gets the full aliases of this <see cref="Command"/>.
        /// </summary>
        /// <remarks>
        ///     Aliases of parent <see cref="Qmmands.Module"/>s and this <see cref="Command"/> concatenated using the <see cref="CommandService.Separator"/>.
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

        internal (bool HasRemainder, string Identifier) SignatureIdentifier { get; }

        internal CommandService Service => Module.Service;

        internal CooldownMap CooldownMap { get; }

        internal Command(CommandBuilder builder, Module module)
        {
            Module = module;

            Description = builder.Description;
            Remarks = builder.Remarks;
            Priority = builder.Priority;
            RunMode = builder.RunMode ?? module.RunMode;
            IgnoreExtraArguments = builder.IgnoreExtraArguments ?? module.IgnoreExtraArguments;
            Callback = builder.Callback;
            Cooldowns = builder.Cooldowns.OrderBy(x => x.Amount).ToImmutableArray();
            Aliases = builder.Aliases.ToImmutableArray();

            var fullAliases = ImmutableArray.CreateBuilder<string>();
            if (Module.FullAliases.Count == 0)
                fullAliases.AddRange(Aliases);

            else if (Aliases.Count == 0)
                fullAliases.AddRange(Module.FullAliases);

            else
            {
                for (var i = 0; i < Module.FullAliases.Count; i++)
                    for (var j = 0; j < Aliases.Count; j++)
                        fullAliases.Add(string.Concat(Module.FullAliases[i], Service.Separator, Aliases[j]));
            }
            FullAliases = fullAliases.TryMoveToImmutable();

            Name = builder.Name ?? (FullAliases.Count > 0 ? FullAliases[0] : null);

            for (var i = 0; i < builder.Checks.Count; i++)
                builder.Checks[i].Command = this;
            Checks = builder.Checks.ToImmutableArray();
            Attributes = builder.Attributes.ToImmutableArray();

            var hasRemainder = false;
            var sb = new StringBuilder();
            var parameters = ImmutableArray.CreateBuilder<Parameter>();
            for (var i = 0; i < builder.Parameters.Count; i++)
            {
                var parameter = builder.Parameters[i].Build(this);
                parameters.Add(parameter);

                if (parameter.IsRemainder)
                    hasRemainder = true;

                sb.Append(parameter.Type).Append(';');
            }
            Parameters = parameters.TryMoveToImmutable();
            SignatureIdentifier = (hasRemainder, sb.ToString());

            if (Cooldowns.Count != 0)
            {
                if (Service.CooldownBucketKeyGenerator is null)
                    throw new CommandBuildingException(builder, "Cooldown bucket key generator delegate has not been set.");

                CooldownMap = new CooldownMap(this);
            }
        }

        /// <summary>
        ///     Runs checks on parent <see cref="Qmmands.Module"/>s and this <see cref="Command"/>.
        /// </summary>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if all of the checks pass, otherwise a <see cref="ChecksFailedResult"/>.
        /// </returns>
        public async Task<IResult> RunChecksAsync(ICommandContext context, IServiceProvider provider = null)
        {
            if (provider is null)
                provider = DummyServiceProvider.Instance;

            var result = await Module.RunChecksAsync(context, provider).ConfigureAwait(false);
            if (!result.IsSuccessful)
                return result;

            if (Checks.Count > 0)
            {
                async Task<(CheckBaseAttribute Check, CheckResult Result)> RunCheckAsync(CheckBaseAttribute check)
                {
                    var checkResult = await check.CheckAsync(context, provider).ConfigureAwait(false);
                    return (check, checkResult);
                }

                var checkResults = await Task.WhenAll(Checks.Select(RunCheckAsync)).ConfigureAwait(false);
                var failedGroups = checkResults.GroupBy(x => x.Check.Group).Where(x => x.Key == null ? x.Any(y => !y.Result.IsSuccessful) : x.All(y => !y.Result.IsSuccessful)).ToImmutableArray();
                if (failedGroups.Length > 0)
                    return new ChecksFailedResult(this, failedGroups.SelectMany(x => x).Where(x => !x.Result.IsSuccessful).ToImmutableArray());
            }

            return new SuccessfulResult();
        }

        /// <summary>
        ///     Resets all <see cref="Cooldown"/> buckets on this <see cref="Command"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     This command does not have any assigned cooldowns.
        /// </exception>
        public void ResetCooldowns()
        {
            if (CooldownMap == null)
                return;

            CooldownMap.Buckets.Clear();
        }

        /// <summary>
        ///     Resets the <see cref="Cooldown"/> bucket with a key generated from the provided
        ///     <see cref="ICommandContext"/> and <see cref="IServiceProvider"/> on this <see cref="Command"/>.
        /// </summary>
        /// <param name="cooldown"> The <see cref="Cooldown"/> to reset. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use for bucket key generation. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use for bucket key generation. </param>
        /// <exception cref="InvalidOperationException">
        ///     This command does not have any assigned cooldowns.
        /// </exception>
        public void ResetCooldown(Cooldown cooldown, ICommandContext context, IServiceProvider provider = null)
        {
            if (CooldownMap == null)
                return;

            if (provider is null)
                provider = DummyServiceProvider.Instance;

            var bucket = CooldownMap.GetBucket(cooldown, context, provider);
            bucket?.Reset();
        }

        /// <summary>
        ///     Runs cooldowns on this <see cref="Command"/>.
        /// </summary>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if no buckets are rate-limited, otherwise a <see cref="CommandOnCooldownResult"/>.
        /// </returns>
        public IResult RunCooldowns(ICommandContext context, IServiceProvider provider = null)
        {
            if (CooldownMap != null)
            {
                if (provider is null)
                    provider = DummyServiceProvider.Instance;

                CooldownMap.Update();
                var buckets = Cooldowns.Select(x => CooldownMap.GetBucket(x, context, provider)).ToArray();
                var rateLimited = ImmutableArray.CreateBuilder<(Cooldown, TimeSpan)>(buckets.Length);
                for (var i = 0; i < buckets.Length; i++)
                {
                    var bucket = buckets[i];
                    if (bucket != null && bucket.IsRateLimited(out var retryAfter))
                        rateLimited.Add((bucket.Cooldown, retryAfter));
                }

                if (rateLimited.Count > 0)
                    return new CommandOnCooldownResult(this, rateLimited.TryMoveToImmutable());

                for (var i = 0; i < buckets.Length; i++)
                    buckets[i]?.Decrement();
            }

            return new SuccessfulResult();
        }

        /// <summary>
        ///     Attempts to parse the raw arguments for this <see cref="Command"/> and execute it.
        ///     Short for <see cref="CommandService.ExecuteAsync(Command, string, ICommandContext, IServiceProvider)"/>
        /// </summary>
        /// <param name="rawArguments"> The raw arguments to use for this <see cref="Command"/>'s <see cref="Parameter"/>s. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The raw arguments must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public Task<IResult> ExecuteAsync(string rawArguments, ICommandContext context, IServiceProvider provider = null)
            => Service.ExecuteAsync(this, rawArguments, context, provider);

        /// <summary>
        ///     Executes this <see cref="Command"/>.
        /// </summary>
        /// <param name="arguments"> The parsed arguments to use for this <see cref="Command"/>'s <see cref="Parameter"/>s. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> to use during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> to use during execution. </param>
        /// <returns>
        ///     An <see cref="IResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The command must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The raw arguments must not be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The context must not be null.
        /// </exception>
        public Task<IResult> ExecuteAsync(object[] arguments, ICommandContext context, IServiceProvider provider = null)
            => Service.ExecuteInternalAsync(this, arguments, context, provider);

        /// <summary>
        ///     Returns <see cref="Name"/> or calls <see cref="object.ToString"/> if it is <see langword="null"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this <see cref="Command"/>.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}