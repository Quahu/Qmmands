using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents the error that occurs when setting <see cref="ModuleBase{TContext}.Context"/> with an unassignable <see cref="CommandContext"/> type.
    /// </summary>
    public sealed class ContextTypeMismatchException : Exception
    {
        /// <summary>
        ///     The expected type of the <see cref="CommandContext"/>.
        /// </summary>
        public Type ExpectedType { get; }

        /// <summary>
        ///     The actual type of the <see cref="CommandContext"/>.
        /// </summary>
        public Type ActualType { get; }

        internal ContextTypeMismatchException(Type expectedType, Type actualType)
            : base($"Unable to set the module's command context. Expected {expectedType}, got {actualType}.")
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }
    }
}
