using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets a priority for the <see cref="Command"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PriorityAttribute : Attribute
    {
        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     Initialises a new <see cref="PriorityAttribute"/> with the specified priority.
        /// </summary>
        /// <param name="priority"> The priority to set. </param>
        public PriorityAttribute(int priority)
            => Priority = priority;
    }
}
