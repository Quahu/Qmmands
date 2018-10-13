using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Adds a check to the <see cref="Parameter"/> that has to succeed before it the <see cref="Command"/> can be executed.
    /// </summary>
    public abstract class ParameterCheckBaseAttribute : Attribute
    {
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
