using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets a name for the <see cref="Module"/>, <see cref="Command"/>, or <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class NameAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Initialises a new <see cref="NameAttribute"/> with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name"> The name to set. </param>
        public NameAttribute(string name)
            => Name = name;
    }
}
