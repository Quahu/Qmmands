using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Module"/> or <see cref="Qmmands.Command"/> check that has to succeed before it can be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class CheckAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.Module"/> this <see cref="CheckAttribute"/> is for.
        ///     <see langword="null"/> if this check is for a <see cref="Qmmands.Command"/>.
        /// </summary>
        public Module Module { get; internal set; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> this <see cref="CheckAttribute"/> is for.
        ///     <see langword="null"/> if this check is for a <see cref="Qmmands.Module"/>.
        /// </summary>
        public Command Command { get; internal set; }

        /// <summary>
        ///     Gets or sets the group for this check.
        /// </summary>
        /// <remarks>
        ///     Grouped checks act as if they were put side by side with the logical OR operator (||) in between.
        /// </remarks>
        public string Group { get; set; }

        /// <summary>
        ///     A method which determines whether the <see cref="Module"/> or <see cref="Command"/> can execute in given circumstances.
        /// </summary>
        /// <param name="context"> The <see cref="CommandContext"/> used during execution. </param>
        /// <returns>
        ///     A <see cref="CheckResult"/> which determines whether this <see cref="CheckAttribute"/> succeeded or not.
        /// </returns>
        public abstract ValueTask<CheckResult> CheckAsync(CommandContext context);
    }
}