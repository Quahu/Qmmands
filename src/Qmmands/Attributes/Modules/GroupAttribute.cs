using System;

namespace Qmmands
{
    /// <summary>
    ///     Adds group aliases to the module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GroupAttribute : Attribute
    {
        /// <summary>
        ///     Gets this group's aliases.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Initialises a new <see cref="GroupAttribute"/> with the specified <paramref name="aliases"/>.
        /// </summary>
        /// <param name="aliases"> The aliases to set. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="aliases"/> mustn't be null. </exception>
        /// <exception cref="ArgumentException"> <paramref name="aliases"/> must contain at least one alias. </exception>
        public GroupAttribute(params string[] aliases)
        {
            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases), "Group aliases mustn't be null.");

            if (aliases.Length == 0)
                throw new ArgumentException("You must provide at least one alias for the group.", nameof(aliases));

            Aliases = aliases;
        }
    }
}
