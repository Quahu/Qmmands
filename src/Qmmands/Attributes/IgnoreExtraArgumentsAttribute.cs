using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets whether to ignore extra arguments for the <see cref="Module"/> or <see cref="Command"/>. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IgnoreExtraArgumentsAttribute : Attribute
    {
        /// <summary>
        ///     Gets whether to ignore extra arguments.
        /// </summary>
        public bool IgnoreExtraArguments { get; }

        /// <summary>
        ///     Initialises a new <see cref="IgnoreExtraArgumentsAttribute"/> with <see cref="IgnoreExtraArguments"/> set to <see langword="true"/>.
        /// </summary>
        public IgnoreExtraArgumentsAttribute()
            => IgnoreExtraArguments = true;

        /// <summary>
        ///     Initialises a new <see cref="IgnoreExtraArgumentsAttribute"/> with the specified <paramref name="ignoreExtraArguments"/>.
        /// </summary>
        /// <param name="ignoreExtraArguments"> The ignore extra arguments value to set. </param>
        public IgnoreExtraArgumentsAttribute(bool ignoreExtraArguments)
            => IgnoreExtraArguments = ignoreExtraArguments;
    }
}
