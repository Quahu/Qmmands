using System;

namespace Qmmands;

/// <summary>
///     Marks the decorated module or command as disabled with an optional reason.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisabledAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the reason for the module or command being disabled.
    /// </summary>
    public string? Reason { get; set; }
}
