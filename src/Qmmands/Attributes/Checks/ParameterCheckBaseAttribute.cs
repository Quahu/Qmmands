using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Adds a check to the <see cref="Parameter"/> that has to succeed before it the <see cref="Command"/> can be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterCheckBaseAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the name of this check.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     Gets or sets the group for this check.
        /// </summary>
        /// <remarks>
        ///     Grouped checks act as if they were put side by side with the logical OR operator (||) in between.
        /// </remarks>
        public string Group { get; set; }

        /// <summary>
        ///     A method which determines whether the <paramref name="argument"/> is valid for the <see cref="Parameter"/> in given circumstances.
        /// </summary>
        /// <param name="parameter"> The currently checked <see cref="Parameter"/>. </param>
        /// <param name="argument"> The value given to this <see cref="Parameter"/>. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> used during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used during execution. </param>
        /// <returns></returns>
        public abstract Task<CheckResult> CheckAsync(Parameter parameter, object argument, ICommandContext context, IServiceProvider provider);
    }
}
