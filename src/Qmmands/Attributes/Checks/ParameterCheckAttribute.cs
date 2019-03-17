﻿using System;
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
        ///     A method which determines whether the <paramref name="argument"/> is valid for the <see cref="Parameter"/> in given circumstances.
        /// </summary>
        /// <param name="argument"> The value given to this <see cref="Parameter"/>. </param>
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
