using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        ///     Gets or sets the <see cref="System.Type"/> of a custom <see cref="TypeParser{T}"/>.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Gets the checks of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<ParameterCheckAttribute> Checks { get; }

        /// <summary>
        ///     Gets the attributes of this <see cref="Parameter"/>.
        /// </summary>
        public IReadOnlyList<Attribute> Attributes { get; }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> of this <see cref="Parameter"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> of this <see cref="Parameter"/>.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="CommandService"/> of this <see cref="Parameter"/>.
        /// </summary>
        public CommandService Service => Command.Service;

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

            if (Type.IsEnum)
                _ = Service.GetPrimitiveTypeParser(Type);

            CustomTypeParserType = builder.CustomTypeParserType;
            for (var i = 0; i < builder.Checks.Count; i++)
            {
                var check = builder.Checks[i];
                if (check.Predicate != null && !check.Predicate(Type))
                    throw new ParameterBuildingException(builder, $"{check} is not a valid parameter check for a parameter of type {Type}.");

                check.Parameter = this;
            }
            Checks = builder.Checks.ToImmutableArray();
            Attributes = builder.Attributes.ToImmutableArray();
        }

        /// <summary>
        ///     Runs checks on this <see cref="Parameter"/>.
        /// </summary>
        /// <param name="argument"> The parsed argument value for this <see cref="Parameter"/>. </param>
        /// <param name="context"> The <see cref="CommandContext"/> used for execution. </param>
        /// <returns>
        ///     A <see cref="SuccessfulResult"/> if all of the checks pass, otherwise a <see cref="ChecksFailedResult"/>.
        /// </returns>
        public async Task<IResult> RunChecksAsync(object argument, CommandContext context)
        {
            if (Checks.Count > 0)
            {
                async Task<(ParameterCheckAttribute Check, CheckResult Result)> RunCheckAsync(ParameterCheckAttribute check)
                {
                    var checkResult = await check.CheckAsync(argument, context).ConfigureAwait(false);
                    return (check, checkResult);
                }

                var checkResults = await Task.WhenAll(Checks.Select(RunCheckAsync)).ConfigureAwait(false);
                var failedGroups = checkResults.GroupBy(x => x.Check.Group)
                    .Where(x => x.Key == null ? x.Any(y => !y.Result.IsSuccessful) : x.All(y => !y.Result.IsSuccessful)).ToImmutableArray();
                if (failedGroups.Length > 0)
                    return new ParameterChecksFailedResult(this, argument, failedGroups.SelectMany(x => x).Where(x => !x.Result.IsSuccessful).ToImmutableArray());
            }

            return new SuccessfulResult();
        }

        /// <summary>
        ///     Returns <see cref="Name"/> or calls <see cref="object.ToString"/> if it is <see langword="null"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="string"/> representing this <see cref="Parameter"/>.
        /// </returns>
        public override string ToString()
            => Name ?? base.ToString();
    }
}
