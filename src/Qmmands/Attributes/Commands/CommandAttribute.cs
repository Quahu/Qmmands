﻿using System;

namespace Qmmands
{
    /// <summary>
    ///     Marks the method as a <see cref="Command"/> with the given aliases.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        ///     Gets the aliases.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Initialises a new <see cref="CommandAttribute"/> with the specified aliases.
        /// </summary>
        /// <param name="aliases"> The aliases to set. </param>
        /// <exception cref="ArgumentNullException">
        ///     Command aliases must not be null.
        /// </exception>
        public CommandAttribute(params string[] aliases)
        {
            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases), "Command aliases must not be null.");

            Aliases = aliases;
        }
    }
}
