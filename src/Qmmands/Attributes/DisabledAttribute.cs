using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets whether the <see cref="Module"/> or <see cref="Command"/> is disabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DisabledAttribute : Attribute
    {
        /// <summary>
        ///     Gets whether to disable the <see cref="Module"/> or <see cref="Command"/>.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="DisabledAttribute"/> with <see cref="Value"/> set to <see langword="true"/>.
        /// </summary>
        public DisabledAttribute()
        {
            Value = true;
        }

        /// <summary>
        ///     Initialises a new <see cref="DisabledAttribute"/> with <see cref="Value"/> set to the specified <paramref name="isDisabled"/>.
        /// </summary>
        /// <param name="isDisabled"> The value to set. </param>
        public DisabledAttribute(bool isDisabled)
        {
            Value = isDisabled;
        }
    }
}
