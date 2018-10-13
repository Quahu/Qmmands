using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets a description for the <see cref="Module"/>, <see cref="Command"/>, or <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class DescriptionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Initialises a new <see cref="DescriptionAttribute"/> with the specified <paramref name="description"/>.
        /// </summary>
        /// <param name="description"> The description to set. </param>
        public DescriptionAttribute(string description)
            => Description = description;
    }
}
