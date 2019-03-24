using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Parameter"/> check that has to succeed before the <see cref="Command"/> can be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterCheckAttribute : Attribute
    {
        /// <summary>
        ///     Gets the types supported by this <see cref="ParameterCheckAttribute"/>.
        /// </summary>
        public IReadOnlyList<Type> SupportedTypes { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> this <see cref="ParameterCheckAttribute"/> is for.
        /// </summary>
        public Parameter Parameter { get; internal set; }

        /// <summary>
        ///     Gets or sets the group for this check.
        /// </summary>
        /// <remarks>
        ///     Grouped checks act as if they were put side by side with the logical OR operator (||) in between.
        /// </remarks>
        public string Group { get; set; }

        /// <summary>
        ///     Initialises a new <see cref="ParameterCheckAttribute"/> with the enumerable of supported <see cref="Type"/>s.
        /// </summary>
        /// <param name="types"> The supported <see cref="Type"/>s. </param>
        protected ParameterCheckAttribute(IEnumerable<Type> types)
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));

            SupportedTypes = types.ToImmutableArray();
        }

        internal ParameterCheckAttribute(ImmutableArray<Type> types)
        {
            SupportedTypes = types;
        }

        /// <summary>
        ///     Determines whether the <paramref name="argument"/> is valid for the <see cref="Qmmands.Parameter"/> in given circumstances.
        /// </summary>
        /// <param name="argument"> The value given to this <see cref="Qmmands.Parameter"/>. </param>
        /// <param name="context"> The <see cref="CommandContext"/> used during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used during execution. </param>
        /// <returns>
        ///     A <see cref="CheckResult"/> which determines whether this <see cref="ParameterCheckAttribute"/> succeeded or not.
        /// </returns>
        public abstract
#if NETCOREAPP
            ValueTask<CheckResult>
#else
            Task<CheckResult>
#endif
            CheckAsync(object argument, CommandContext context, IServiceProvider provider);
    }
}
