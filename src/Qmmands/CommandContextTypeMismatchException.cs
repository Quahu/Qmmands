using System;

namespace Qmmands;

/// <summary>
///     Represents the error that occurs when setting <see cref="IModuleBase.Context"/> with an unassignable <see cref="ICommandContext"/> type.
/// </summary>
public class CommandContextTypeMismatchException : Exception
{
    /// <summary>
    ///     Gets the expected type of the command context.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    ///     Gets the actual type of the command context.
    /// </summary>
    public Type ActualType { get; }

    /// <summary>
    ///     Gets the type of the module base.
    /// </summary>
    public Type ModuleType { get; }

    /// <summary>
    ///     Instantiates a new <see cref="CommandContextTypeMismatchException"/> with the specified types.
    /// </summary>
    /// <param name="expectedType"> The expected type of the command context. </param>
    /// <param name="actualType"> The actual type of the command context. </param>
    /// <param name="moduleType"> The type of the module base. </param>
    public CommandContextTypeMismatchException(Type expectedType, Type actualType, Type moduleType)
        : this(expectedType, actualType, moduleType, $"The command context of type '{actualType}' is not assignable to the type '{expectedType}' which '{moduleType}' requires.")
    { }

    /// <summary>
    ///     Instantiates a new <see cref="CommandContextTypeMismatchException"/> with the specified types and error message.
    /// </summary>
    /// <param name="expectedType"> The expected type of the command context. </param>
    /// <param name="actualType"> The actual type of the command context. </param>
    /// <param name="moduleType"> The type of the module base. </param>
    /// <param name="message"> The error message. </param>
    protected CommandContextTypeMismatchException(Type expectedType, Type actualType, Type moduleType, string message)
        : base(message)
    {
        ExpectedType = expectedType;
        ActualType = actualType;
        ModuleType = moduleType;
    }
}