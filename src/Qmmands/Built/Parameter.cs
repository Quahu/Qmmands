using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a parameter built using the <see cref="CommandService"/>.
    /// </summary>
    public sealed class Parameter
    {
        /// <summary>
        ///     Gets the name of this <see cref="Parameter"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this <see cref="Parameter"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the remarks of this <see cref="Parameter"/>.
        /// </summary>
        public string Remarks { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is a remainder parameter or not.
        /// </summary>
        public bool IsRemainder { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is multiple or not.
        /// </summary>
        public bool IsMultiple { get; }

        /// <summary>
        ///     Gets whether this <see cref="Parameter"/> is optional or not.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        ///     Gets the default value of this <see cref="Parameter"/>.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> of this <see cref="Parameter"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the custom <see cref="TypeParser{T}"/>'s type of this <see cref="Parameter"/>.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Gets the checks of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<ParameterCheckBaseAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> of this <see cref="Parameter"/>.
        /// </summary>
        public Command Command { get; }

        internal CommandService Service => Command.Service;

        internal Parameter(ParameterBuilder builder, Command command)
        {
            Command = command;

            Name = builder.Name;
            Description = builder.Description;
            Remarks = builder.Remarks;
            IsRemainder = builder.IsRemainder;
            IsMultiple = builder.IsMultiple;
            IsOptional = builder.IsOptional;
            DefaultValue = builder.DefaultValue;
            Type = builder.Type;

            if (Type == null)
                throw new InvalidOperationException("Parameter must have an assigned type.");

            if (IsOptional)
            {
                if (DefaultValue is null)
                {
                    if (Type.IsValueType && !ReflectionUtilities.IsNullable(Type))
                        throw new InvalidOperationException("A value type parameter can't have null as the default value.");
                }

                else if (DefaultValue.GetType() != Type)
                    throw new InvalidOperationException($"Parameter type and default value mismatch. Expected {Type}, got {DefaultValue.GetType()}.");
            }

            if (Type.IsEnum)
                _ = Service.GetPrimitiveTypeParser(Type);

            if (builder.CustomTypeParserType != null)
            {
                if (!ReflectionUtilities.IsValidParserDefinition(builder.CustomTypeParserType.GetTypeInfo(), Type))
                    throw new InvalidOperationException($"{builder.CustomTypeParserType.Name} isn't a valid type parser for parameter of type {Type}.");

                CustomTypeParserType = builder.CustomTypeParserType;
            }

            for (var i = 0; i < builder.Checks.Count; i++)
                builder.Checks[i].Parameter = this;
            Checks = builder.Checks.ToImmutableArray();
            Attributes = builder.Attributes.ToImmutableArray();
        }

        /// <summary>
        ///     Runs parameter checks on this <see cref="Parameter"/>.
        /// </summary>
        /// <param name="argument"> The parsed argument value for this <see cref="Parameter"/>. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if all of the checks pass, otherwise a <see cref="ChecksFailedResult"/>.
        /// </returns>
        public async Task<IResult> RunChecksAsync(object argument, ICommandContext context, IServiceProvider provider = null)
        {
            if (provider is null)
                provider = DummyServiceProvider.Instance;

            if (Checks.Count > 0)
            {
                var checkResults = await Task.WhenAll(Checks.Select(x => RunCheckAsync(x, argument, context, provider))).ConfigureAwait(false);
                var failedGroups = checkResults.GroupBy(x => x.Check.Group).Where(x => x.Key == null ? x.Any(y => y.Error != null) : x.All(y => y.Error != null)).ToImmutableArray();
                if (failedGroups.Length > 0)
                    return new ParameterChecksFailedResult(this, failedGroups.SelectMany(x => x).Where(x => x.Error != null).ToImmutableArray());
            }

            return new SuccessfulResult();
        }

        private async Task<(ParameterCheckBaseAttribute Check, string Error)> RunCheckAsync(ParameterCheckBaseAttribute check, object argument, ICommandContext context, IServiceProvider provider)
        {
            var checkResult = await check.CheckAsync(argument, context, provider).ConfigureAwait(false);
            return (check, checkResult.Reason);
        }

        /// <summary>
        ///     Returns <see cref="Name"/> or calls <see cref="object.ToString"/> if it's <see langword="null"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this <see cref="Parameter"/>.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}
