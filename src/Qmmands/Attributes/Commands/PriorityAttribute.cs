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
        public int Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="PriorityAttribute"/> with the specified priority.
        /// </summary>
        /// <param name="priority"> The priority to set. </param>
        /// <remarks>
        ///    The <see cref="CommandService"/> will try to execute higher priority <see cref="Command"/>s first.
        /// </remarks>
        public PriorityAttribute(int priority)
        {
            Value = priority;
        }
    }
}
