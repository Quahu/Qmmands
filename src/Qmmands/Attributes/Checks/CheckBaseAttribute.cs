using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Adds a check to the <see cref="Module"/> or <see cref="Command"/> that has to succeed before it can be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class CheckBaseAttribute : Attribute
    {
        /// <summary>
        ///     Sets the group for this check.
        /// </summary>
        /// <remarks>
        ///     Grouped checks act as if they were put side by side with the logical OR operator (||) in between.
        /// </remarks>
        public string Group { get; set; }

        /// <summary>
        ///     A method which determines whether the <see cref="Module"/> or <see cref="Command"/> can execute in given circumstances.
        /// </summary>
        /// <param name="context"> The <see cref="ICommandContext"/> used during execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used during execution. </param>
        /// <returns> A <see cref="CheckResult"/> which determines whether this check failed or succeeded. </returns>
        public abstract Task<CheckResult> CheckAsync(ICommandContext context, IServiceProvider provider);
    }
}