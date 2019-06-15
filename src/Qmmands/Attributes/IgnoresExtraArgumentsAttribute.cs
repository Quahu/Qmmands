using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets whether to ignore extra arguments for the <see cref="Module"/> or <see cref="Command"/>. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IgnoresExtraArgumentsAttribute : Attribute
    {
        /// <summary>
        ///     Gets whether to ignore extra arguments.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="IgnoresExtraArgumentsAttribute"/> with <see cref="Value"/> set to <see langword="true"/>.
        /// </summary>
        public IgnoresExtraArgumentsAttribute()
        {
            Value = true;
        }

        /// <summary>
        ///     Initialises a new <see cref="IgnoresExtraArgumentsAttribute"/> with the specified <paramref name="ignoreExtraArguments"/>.
        /// </summary>
        /// <param name="ignoreExtraArguments"> The value to set. </param>
        public IgnoresExtraArgumentsAttribute(bool ignoreExtraArguments)
        {
            Value = ignoreExtraArguments;
        }
    }
}
